using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Features.Notlar.Commands;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotlarController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AppDbContext _context;

        public NotlarController(IMediator mediator, AppDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpPost("Kaydet")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> SaveNot([FromBody] NotGirisDto dto)
        {
            if (dto.Vize is < 0 or > 100 || dto.Final is < 0 or > 100) return BadRequest("Notlar 0 ile 100 arasında olmalıdır.");
            var course = await _context.Dersler.FirstOrDefaultAsync(d => d.DersKodu == dto.DersKodu);
            if (course == null) return NotFound("Ders bulunamadı.");
            if (!User.IsInRole("Admin") && !string.Equals(course.AkademisyenEmail, User.FindFirstValue(ClaimTypes.Email), StringComparison.OrdinalIgnoreCase)) return Forbid();
            var result = await _mediator.Send(new SaveNotCommand { NotDto = dto });
            if (!result) return BadRequest("Not kaydı başarısız.");
            return Ok(new { message = "Not başarıyla kaydedildi." });
        }

        [HttpGet("OgrenciNotlari")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetOgrenciNotlari([FromQuery] string email)
        {
            if (!User.IsInRole("Admin") && !string.Equals(User.FindFirstValue(ClaimTypes.Email), email, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var student = await _context.Ogrenciler.FirstOrDefaultAsync(u => u.Email == email);
            if (student == null) return NotFound("Öğrenci bulunamadı.");

            // DÜZELTME 1: studentIdStr kaldırıldı, doğrudan int olan student.Id kullanılıyor
            var kayitliDersler = await _context.OgrenciDersler
                .Where(od => od.OgrenciId == student.Id)
                .Include(od => od.Ders)
                .ToListAsync();

            var notlar = await _context.Notlar
                .Where(n => n.StudentId == student.Id)
                .ToListAsync();

            var sonuc = kayitliDersler.Select(od =>
            {
                var not = notlar.FirstOrDefault(n => n.DersId == od.DersId);
                return new
                {
                    DersKodu = od.Ders.DersKodu,
                    DersAdi = od.Ders.Adi,
                    Akts = od.Ders.Akts,
                    Sinif = od.Ders.Sinif,
                    Donem = od.Ders.Donem.ToString(),
                    Vize = not?.Vize,
                    Final = not?.Final,
                    Ortalama = not?.Ortalama,
                    HarfNotu = not?.HarfNotu
                };
            }).ToList();

            return Ok(sonuc);
        }

        // 1. Akademisyenin Verdiği Dersleri Listeleme
        [HttpGet("AkademisyenDersleri")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> GetAkademisyenDersleri([FromQuery] string email)
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!User.IsInRole("Admin") && !string.Equals(currentEmail, email, StringComparison.OrdinalIgnoreCase))
                return Forbid();
            var dersler = await _context.Dersler
                .Where(d => d.AkademisyenEmail == email)
                .Select(d => new { id = d.Id, kod = d.DersKodu, ad = d.Adi })
                .ToListAsync();

            return Ok(dersler);
        }

        // 2. Seçilen Dersi Alan Öğrencileri ve Varsa Mevcut Notlarını Getirme
        [HttpGet("DersOgrencileri")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> GetDersOgrencileri([FromQuery] int dersId)
        {
            if (!await CanManageCourse(dersId)) return Forbid();
            var ogrenciler = await _context.OgrenciDersler
                .Where(od => od.DersId == dersId)
                .Include(od => od.Ogrenci)
                .Select(od => od.Ogrenci)
                .ToListAsync();
            var notlar = await _context.Notlar.Where(n => n.DersId == dersId).ToListAsync();

            var sonuc = ogrenciler.Select(o => {
                var notKaydi = notlar.FirstOrDefault(n => n.StudentId == o.Id);
                return new
                {
                    ogrenciId = o.Id,
                    ogrenciNumarasi = o.OgrenciNumarasi,
                    ad = o.Adi,
                    soyad = o.Soyadi,
                    vize = notKaydi?.Vize,
                    final = notKaydi?.Final
                };
            }).ToList();

            return Ok(sonuc);
        }

        // 3. Toplu Not Kaydetme Endpoint'i
        [HttpPost("TopluKaydet")]
        [Authorize(Roles = "Admin,Lecturer")]
        public async Task<IActionResult> TopluKaydet([FromBody] TopluNotGirisDto dto)
        {
            if (dto.DersId <= 0 || dto.Notlar == null || dto.Notlar.Count == 0)
                return BadRequest("Ders ve en az bir öğrenci notu gönderilmelidir.");

            if (!await _context.Dersler.AnyAsync(d => d.Id == dto.DersId)) return NotFound("Ders bulunamadı.");
            if (!await CanManageCourse(dto.DersId)) return Forbid();

            var kayitliOgrenciIdleri = (await _context.OgrenciDersler
                .Where(od => od.DersId == dto.DersId)
                .Select(od => od.OgrenciId)
                .ToListAsync()).ToHashSet();

            var gecersizOgrenciler = dto.Notlar
                .Where(n => n.OgrenciId <= 0 || !kayitliOgrenciIdleri.Contains(n.OgrenciId))
                .Select(n => n.OgrenciId)
                .Distinct()
                .ToList();

            if (gecersizOgrenciler.Any())
                return BadRequest(new { message = "Not girilecek öğrenci bu dersi seçmemiş veya bulunamadı.", ogrenciIdleri = gecersizOgrenciler });

            if (dto.Notlar.Any(n => (n.Vize.HasValue && (n.Vize < 0 || n.Vize > 100)) ||
                                    (n.Final.HasValue && (n.Final < 0 || n.Final > 100))))
                return BadRequest("Vize ve final notu 0 ile 100 arasında olmalıdır.Lütfen geçerli bir değer giriniz.");

            var mevcutNotlar = await _context.Notlar
                .Where(n => n.DersId == dto.DersId && kayitliOgrenciIdleri.Contains(n.StudentId))
                .ToDictionaryAsync(n => n.StudentId);

            foreach (var item in dto.Notlar)
            {
                mevcutNotlar.TryGetValue(item.OgrenciId, out var notKaydi);

                if (notKaydi == null)
                {
                    notKaydi = new Not
                    {
                        DersId = dto.DersId,                       
                        StudentId = item.OgrenciId
                    };
                    _context.Notlar.Add(notKaydi);
                }

                notKaydi.Vize = item.Vize;
                notKaydi.Final = item.Final;

                if (item.Vize.HasValue && item.Final.HasValue)
                {
                    decimal ortalama = (item.Vize.Value * 0.4m) + (item.Final.Value * 0.6m);
                    notKaydi.Ortalama = ortalama;
                    notKaydi.HarfNotu = CalculateHarfNotu(ortalama);
                }
                else
                {
                    notKaydi.Ortalama = null;
                    notKaydi.HarfNotu = string.Empty;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Not kayıt işlemi başarılı." });
        }

        private string CalculateHarfNotu(decimal ort) => ort switch
        {
            >= 90 => "AA",
            >= 85 => "BA",
            >= 80 => "BB",
            >= 75 => "CB",
            >= 70 => "CC",
            >= 65 => "DC",
            >= 60 => "DD",
            >= 50 => "FD",
            _ => "FF"
        };

        private async Task<bool> CanManageCourse(int dersId)
        {
            if (User.IsInRole("Admin")) return true;
            var email = User.FindFirstValue(ClaimTypes.Email);
            return !string.IsNullOrWhiteSpace(email) && await _context.Dersler
                .AnyAsync(d => d.Id == dersId && d.AkademisyenEmail == email);
        }
    }
}

