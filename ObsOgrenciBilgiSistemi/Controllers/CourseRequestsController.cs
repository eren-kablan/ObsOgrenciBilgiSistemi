using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController]
[Route("api/course-requests")]
[Authorize]
public class CourseRequestsController : ControllerBase
{
    private readonly AppDbContext _context;
    public CourseRequestsController(AppDbContext context) => _context = context;

    // Akademisyene açık dersler
    [HttpGet("available")]
    [Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
    {
        var lecturer = await GetCurrentLecturer(cancellationToken);
        if (lecturer is null) return NotFound(new { message = "Akademisyen profili bulunamadı." });

        var pendingCourseIds = _context.DersTalepleri
            .Where(request => request.AkademisyenId == lecturer.Id && request.Durum == "Bekliyor")
            .Select(request => request.DersId);

        var courses = await _context.Dersler
            .Where(course => course.BolumId == lecturer.BolumId
                && course.AkademisyenId == null
                && !pendingCourseIds.Contains(course.Id))
            .OrderBy(course => course.DersKodu)
            .Select(course => new
            {
                course.Id,
                course.DersKodu,
                course.Adi,
                course.Kredi,
                course.Akts,
                course.Sinif,
                course.Donem
            })
            .ToListAsync(cancellationToken);

        return Ok(courses);
    }

    // Akademisyenin talepleri
    [HttpGet("mine")]
    [Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var email = CurrentEmail();
        var requests = await _context.DersTalepleri
            .Where(request => request.AkademisyenEmail == email)
            .OrderByDescending(request => request.TalepTarihi)
            .Select(request => new
            {
                request.Id,
                request.DersId,
                request.Ders.DersKodu,
                DersAdi = request.Ders.Adi,
                request.Durum,
                request.TalepTarihi,
                request.KararTarihi
            })
            .ToListAsync(cancellationToken);
        return Ok(requests);
    }

    [HttpPost]
    [Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> Create(CreateCourseRequestDto dto, CancellationToken cancellationToken)
    {
        var lecturer = await GetCurrentLecturer(cancellationToken);
        if (lecturer is null) return NotFound(new { message = "Akademisyen profili bulunamadı." });

        var course = await _context.Dersler.FirstOrDefaultAsync(course => course.Id == dto.CourseId, cancellationToken);
        if (course is null) return NotFound(new { message = "Ders bulunamadı." });
        if (course.BolumId != lecturer.BolumId) return Forbid();
        if (course.AkademisyenId is not null) return Conflict(new { message = "Bu ders zaten bir akademisyene atanmış." });

        bool alreadyPending = await _context.DersTalepleri.AnyAsync(
            request => request.DersId == dto.CourseId
                && request.AkademisyenId == lecturer.Id
                && request.Durum == "Bekliyor",
            cancellationToken);
        if (alreadyPending) return Conflict(new { message = "Bu ders için zaten bekleyen talebiniz var." });

        var request = new DersTalebi
        {
            DersId = course.Id,
            AkademisyenId = lecturer.Id,
            AkademisyenEmail = lecturer.Email,
            Durum = "Bekliyor"
        };
        _context.DersTalepleri.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = $"{course.DersKodu} dersi için talebiniz oluşturuldu.", request.Id });
    }

    // Admin onay listesi
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var requests = await _context.DersTalepleri
            .Where(request => request.Durum == "Bekliyor")
            .OrderBy(request => request.TalepTarihi)
            .Select(request => new
            {
                request.Id,
                request.DersId,
                request.Ders.DersKodu,
                DersAdi = request.Ders.Adi,
                BolumAdi = request.Ders.Bolum.Adi,
                request.AkademisyenId,
                request.AkademisyenEmail,
                AkademisyenAdi = request.Akademisyen.Unvani + " " + request.Akademisyen.Adi + " " + request.Akademisyen.Soyadi,
                request.TalepTarihi
            })
            .ToListAsync(cancellationToken);
        return Ok(requests);
    }

    // Talep kararı
    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Approve(int id, CancellationToken cancellationToken) =>
        Decide(id, approve: true, cancellationToken);

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public Task<IActionResult> Reject(int id, CancellationToken cancellationToken) =>
        Decide(id, approve: false, cancellationToken);

    private async Task<IActionResult> Decide(int id, bool approve, CancellationToken cancellationToken)
    {
        var request = await _context.DersTalepleri
            .Include(item => item.Ders)
            .Include(item => item.Akademisyen)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (request is null) return NotFound(new { message = "Ders talebi bulunamadı." });
        if (request.Durum != "Bekliyor") return Conflict(new { message = "Bu talep daha önce sonuçlandırılmış." });

        var now = DateTime.UtcNow;
        request.Durum = approve ? "Onaylandı" : "Reddedildi";
        request.KararTarihi = now;
        request.KararVerenEmail = CurrentEmail();

        if (approve)
        {
            if (request.Ders.AkademisyenId is not null)
                return Conflict(new { message = "Ders başka bir akademisyene atanmış." });

            request.Ders.AkademisyenId = request.AkademisyenId;
            request.Ders.AkademisyenEmail = request.AkademisyenEmail;
            AddNotification(request.AkademisyenEmail, "Ders talebiniz onaylandı",
                $"Artık {request.Ders.DersKodu} - {request.Ders.Adi} dersini veriyorsunuz.");

            var competingRequests = await _context.DersTalepleri
                .Where(item => item.DersId == request.DersId && item.Id != request.Id && item.Durum == "Bekliyor")
                .ToListAsync(cancellationToken);
            foreach (var competing in competingRequests)
            {
                competing.Durum = "Reddedildi";
                competing.KararTarihi = now;
                competing.KararVerenEmail = CurrentEmail();
                AddNotification(competing.AkademisyenEmail, "Ders talebiniz reddedildi",
                    $"{request.Ders.DersKodu} - {request.Ders.Adi} dersi başka bir akademisyene atandı.");
            }
        }
        else
        {
            AddNotification(request.AkademisyenEmail, "Ders talebiniz reddedildi",
                $"{request.Ders.DersKodu} - {request.Ders.Adi} ders talebiniz reddedildi.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = approve ? "Ders talebi onaylandı." : "Ders talebi reddedildi." });
    }

    // Bildirim ve kullanıcı yardımcıları
    private void AddNotification(string recipientEmail, string title, string message) =>
        _context.Bildirimler.Add(new Bildirim { AliciEmail = recipientEmail, Baslik = title, Mesaj = message });

    private string CurrentEmail() => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    private Task<Akademisyen?> GetCurrentLecturer(CancellationToken cancellationToken) =>
        _context.Akademisyenler.FirstOrDefaultAsync(
            lecturer => lecturer.Email == CurrentEmail(),
            cancellationToken);
}

public record CreateCourseRequestDto(int CourseId);
