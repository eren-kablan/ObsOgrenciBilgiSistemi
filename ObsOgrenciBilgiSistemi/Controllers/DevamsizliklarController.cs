using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using System.Data;
using System.Security.Claims;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class DevamsizliklarController : ControllerBase
{
    private readonly AppDbContext _context;
    public DevamsizliklarController(AppDbContext context) => _context = context;

    // Akademisyen devamsızlık ekranı
    [HttpGet("akademisyen-dersleri"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> LecturerCourses()
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(await _context.Dersler.AsNoTracking().Where(course => course.AkademisyenEmail == email)
            .OrderBy(course => course.DersKodu)
            .Select(course => new { course.Id, course.DersKodu, DersAdi = course.Adi, course.Sinif, course.Akts }).ToListAsync());
    }

    [HttpGet("ders/{courseId:int}/hafta/{week:int}"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> CourseWeek(int courseId, int week)
    {
        if (week is < 1 or > 12) return BadRequest(new { message = "Hafta 1 ile 12 arasında olmalıdır." });
        if (!await OwnsCourse(courseId)) return Forbid();
        var students = await _context.OgrenciDersler.AsNoTracking().Where(registration => registration.DersId == courseId)
            .OrderBy(registration => registration.Ogrenci.Adi).ThenBy(registration => registration.Ogrenci.Soyadi)
            .Select(registration => new { registration.Ogrenci.Id, registration.Ogrenci.Adi, registration.Ogrenci.Soyadi, registration.Ogrenci.OgrenciNumarasi }).ToListAsync();
        var records = await _context.Devamsizliklar.AsNoTracking().Where(item => item.DersId == courseId && item.DevamsizlikHaftasi == week)
            .ToDictionaryAsync(item => item.StudentId, item => item.Durum);
        return Ok(students.Select(student => new
        {
            student.Id, student.Adi, student.Soyadi, student.OgrenciNumarasi,
            Katildi = !records.TryGetValue(student.Id, out bool attended) || attended
        }));
    }

    public record SaveWeekRequest(int CourseId, int Week, List<int> AbsentStudentIds);

    // Öğrenci devamsızlık ekranı
    [HttpGet("ogrenci-dersleri"), Authorize(Roles = "Student,ogrenci,Öğrenci")]
    public async Task<IActionResult> StudentCourses()
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var student = await _context.Ogrenciler.AsNoTracking().FirstOrDefaultAsync(item => item.Email == email);
        if (student == null) return NotFound(new { message = "Öğrenci profili bulunamadı." });
        return Ok(await _context.OgrenciDersler.AsNoTracking().Where(item => item.OgrenciId == student.Id)
            .OrderBy(item => item.Ders.DersKodu).Select(item => new
            {
                item.Ders.Id, item.Ders.DersKodu, DersAdi = item.Ders.Adi, item.Ders.Akts,
                IslenenHafta = _context.Devamsizliklar.Count(record => record.StudentId == student.Id && record.DersId == item.DersId),
                DevamsizHafta = _context.Devamsizliklar.Count(record => record.StudentId == student.Id && record.DersId == item.DersId && !record.Durum)
            }).ToListAsync());
    }

    [HttpGet("ogrenci-ders/{courseId:int}"), Authorize(Roles = "Student,ogrenci,Öğrenci")]
    public async Task<IActionResult> StudentCourseWeeks(int courseId)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        var student = await _context.Ogrenciler.AsNoTracking().FirstOrDefaultAsync(item => item.Email == email);
        if (student == null) return NotFound(new { message = "Öğrenci profili bulunamadı." });
        bool registered = await _context.OgrenciDersler.AnyAsync(item => item.OgrenciId == student.Id && item.DersId == courseId);
        if (!registered) return Forbid();
        var records = await _context.Devamsizliklar.AsNoTracking()
            .Where(item => item.StudentId == student.Id && item.DersId == courseId)
            .ToDictionaryAsync(item => item.DevamsizlikHaftasi);
        return Ok(Enumerable.Range(1, 12).Select(week =>
        {
            records.TryGetValue(week, out var record);
            return new { Hafta = week, Durum = record == null ? "Islenmedi" : record.Durum ? "Katildi" : "Gelmedi", Tarih = record?.Tarih };
        }));
    }

    // Haftalık devamsızlık kaydı
    [HttpPut("hafta"), Authorize(Roles = "Lecturer,Akademisyen")]
    public async Task<IActionResult> SaveWeek(SaveWeekRequest request)
    {
        if (request.Week is < 1 or > 12) return BadRequest(new { message = "Hafta 1 ile 12 arasında olmalıdır." });
        if (!await OwnsCourse(request.CourseId)) return Forbid();

       
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<IActionResult>(async () =>
        {
           
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var registeredIds = await _context.OgrenciDersler
                .Where(item => item.DersId == request.CourseId)
                .Select(item => item.OgrenciId)
                .Distinct()
                .ToListAsync();
            var absentIds = (request.AbsentStudentIds ?? []).Distinct().ToHashSet();
            if (absentIds.Except(registeredIds).Any())
                return BadRequest(new { message = "Derse kayıtlı olmayan öğrenci için işlem yapılamaz." });

            var existing = await _context.Devamsizliklar
                .Where(item => item.DersId == request.CourseId && item.DevamsizlikHaftasi == request.Week)
                .ToDictionaryAsync(item => item.StudentId);
            foreach (int studentId in registeredIds)
            {
                bool attended = !absentIds.Contains(studentId);
                if (existing.TryGetValue(studentId, out var record))
                {
                    record.Durum = attended;
                    record.Tarih = DateTime.Now;
                }
                else
                {
                    _context.Devamsizliklar.Add(new Devamsizlik
                    {
                        StudentId = studentId,
                        DersId = request.CourseId,
                        DevamsizlikHaftasi = request.Week,
                        Durum = attended,
                        Tarih = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { message = $"{request.Week}. hafta devamsızlığı kaydedildi.", absentCount = absentIds.Count });
        });
    }

    private async Task<bool> OwnsCourse(int courseId)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email);
        return await _context.Dersler.AnyAsync(course => course.Id == courseId && course.AkademisyenEmail == email);
    }
}
