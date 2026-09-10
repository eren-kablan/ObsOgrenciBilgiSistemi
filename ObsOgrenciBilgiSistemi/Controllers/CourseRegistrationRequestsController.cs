using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using System.Security.Claims;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController, Route("api/course-registration-requests"), Authorize]
public class CourseRegistrationRequestsController : ControllerBase
{
    private readonly AppDbContext _context;
    public CourseRegistrationRequestsController(AppDbContext context) => _context = context;

    public record SubmitRequest(List<int> CourseIds, string AcademicYear, int Term);
    public record RejectRequest(string Reason);

    // Öğrenci ders kayıt talebi
    [HttpPost, Authorize(Roles = "Student,ogrenci,Öğrenci")]
    public async Task<IActionResult> Submit(SubmitRequest request)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var student = await _context.Ogrenciler.Include(item => item.Bolum)
            .FirstOrDefaultAsync(item => item.Email == email);
        if (student == null) return NotFound(new { message = "Öğrenci bulunamadı." });
        if (student.Bolum.DanismanAkademisyenId == null)
            return BadRequest(new { message = "Bölümünüze henüz danışman akademisyen atanmamış. Lütfen öğrenci işleriyle iletişime geçin." });
        var courseIds = request.CourseIds.Distinct().ToList();
        if (courseIds.Count == 0) return BadRequest(new { message = "En az bir ders seçmelisiniz." });
        if (request.Term is < 1 or > 2 || string.IsNullOrWhiteSpace(request.AcademicYear))
            return BadRequest(new { message = "Akademik dönem bilgisi geçersiz." });
        bool locked = await _context.DersKayitTalepleri.AnyAsync(item => item.OgrenciId == student.Id &&
            item.AkademikYil == request.AcademicYear && item.Donem == request.Term &&
            (item.Durum == DersKayitDurumu.Bekliyor || item.Durum == DersKayitDurumu.Onaylandi));
        if (locked) return Conflict(new { message = "Bu dönem için bekleyen veya onaylanmış bir ders kaydınız bulunuyor." });
        var courses = await _context.Dersler.Where(course => courseIds.Contains(course.Id)).ToListAsync();
        if (courses.Count != courseIds.Count || courses.Any(course => course.BolumId != student.BolumId ||
            course.Sinif != student.Sinif || (int)course.Donem != request.Term))
            return BadRequest(new { message = "Yalnızca kendi bölüm, sınıf ve aktif dönem derslerinizi seçebilirsiniz." });
        int registeredAkts = await _context.OgrenciDersler.Where(item => item.OgrenciId == student.Id)
            .SumAsync(item => item.Ders.Akts);
        if (registeredAkts + courses.Sum(course => course.Akts) > 40)
            return BadRequest(new { message = "Dönemlik toplam ders yükü 40 AKTS'yi aşamaz." });

        var registration = new DersKayitTalebi
        {
            OgrenciId = student.Id,
            DanismanAkademisyenId = student.Bolum.DanismanAkademisyenId.Value,
            AkademikYil = request.AcademicYear.Trim(), Donem = request.Term,
            Dersler = courses.Select(course => new DersKayitTalepDersi { DersId = course.Id }).ToList()
        };
        _context.DersKayitTalepleri.Add(registration);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Ders seçiminiz danışman onayına gönderildi.", registration.Id });
    }

    // Talep durumu
    [HttpGet("status"), Authorize(Roles = "Student,ogrenci,Öğrenci")]
    public async Task<IActionResult> Status([FromQuery] string academicYear, [FromQuery] int term)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var result = await _context.DersKayitTalepleri.AsNoTracking()
            .Where(item => item.Ogrenci.Email == email && item.AkademikYil == academicYear && item.Donem == term)
            .OrderByDescending(item => item.TalepTarihi).Select(item => new
            {
                item.Id, Durum = item.Durum.ToString(), item.RedNedeni, item.TalepTarihi, item.KararTarihi,
                Danisman = item.DanismanAkademisyen.Unvani + " " + item.DanismanAkademisyen.Adi + " " + item.DanismanAkademisyen.Soyadi
            }).FirstOrDefaultAsync();
        return Ok(result);
    }

    // Danışman onay listesi
    [HttpGet("pending"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> Pending()
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(await _context.DersKayitTalepleri.AsNoTracking()
            .Where(item => item.DanismanAkademisyen.Email == email && item.Durum == DersKayitDurumu.Bekliyor)
            .OrderBy(item => item.TalepTarihi).Select(item => new
            {
                item.Id, item.AkademikYil, item.Donem, item.TalepTarihi,
                OgrenciAdi = item.Ogrenci.Adi + " " + item.Ogrenci.Soyadi,
                item.Ogrenci.OgrenciNumarasi, Sinif = item.Ogrenci.Sinif,
                Dersler = item.Dersler.Select(selected => new { selected.Ders.Id, selected.Ders.DersKodu, DersAdi = selected.Ders.Adi, selected.Ders.Akts })
            }).ToListAsync());
    }

    // Onay ve ret işlemleri
    [HttpPost("{id:int}/approve"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> Approve(int id) => await Decide(id, true, null);

    [HttpPost("{id:int}/reject"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> Reject(int id, RejectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { message = "Ret gerekçesi zorunludur." });
        return await Decide(id, false, request.Reason.Trim());
    }

    private async Task<IActionResult> Decide(int id, bool approve, string? reason)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var registration = await _context.DersKayitTalepleri.Include(item => item.Ogrenci).Include(item => item.Dersler)
            .ThenInclude(item => item.Ders).FirstOrDefaultAsync(item => item.Id == id && item.DanismanAkademisyen.Email == email);
        if (registration == null) return NotFound(new { message = "Ders kayıt talebi bulunamadı." });
        if (registration.Durum != DersKayitDurumu.Bekliyor) return Conflict(new { message = "Bu talep daha önce sonuçlandırılmış." });
        if (approve)
        {
            var existingIds = await _context.OgrenciDersler.Where(item => item.OgrenciId == registration.OgrenciId)
                .Select(item => item.DersId).ToListAsync();
            foreach (var selected in registration.Dersler.Where(item => !existingIds.Contains(item.DersId)))
                _context.OgrenciDersler.Add(new OgrenciDers { OgrenciId = registration.OgrenciId, DersId = selected.DersId });
            registration.Durum = DersKayitDurumu.Onaylandi;
        }
        else { registration.Durum = DersKayitDurumu.Reddedildi; registration.RedNedeni = reason; }
        registration.KararTarihi = DateTime.UtcNow;
        _context.Bildirimler.Add(new Bildirim
        {
            AliciEmail = registration.Ogrenci.Email,
            Baslik = approve ? "Ders Kaydınız Onaylandı" : "Ders Kaydınız Reddedildi",
            Mesaj = approve ? $"{registration.AkademikYil} {registration.Donem}. dönem ders seçiminiz danışmanınız tarafından onaylandı."
                : $"Ders seçiminiz danışmanınız tarafından reddedildi. Gerekçe: {reason}"
        });
        await _context.SaveChangesAsync();
        return Ok(new { message = approve ? "Ders kaydı onaylandı." : "Ders kaydı reddedildi." });
    }
}
