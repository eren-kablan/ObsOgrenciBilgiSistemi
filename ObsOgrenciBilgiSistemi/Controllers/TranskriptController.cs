using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController, Authorize]
[Route("api/[controller]")]
public class TranskriptController : ControllerBase
{
    private readonly AppDbContext _context;
    public TranskriptController(AppDbContext context) => _context = context;

    [HttpGet("Ozet")]
    public async Task<IActionResult> Ozet([FromQuery] string email)
    {
        if (!User.IsInRole("Admin") && !string.Equals(User.FindFirstValue(ClaimTypes.Email), email, StringComparison.OrdinalIgnoreCase)) return Forbid();
        var student = await _context.Ogrenciler.FirstOrDefaultAsync(u => u.Email == email);
        if (student is null) return NotFound("Öğrenci bulunamadı.");

        var points = new Dictionary<string, decimal> { ["AA"] = 4, ["BA"] = 3.5m, ["BB"] = 3, ["CB"] = 2.5m, ["CC"] = 2, ["DC"] = 1.5m, ["DD"] = 1, ["FD"] = .5m, ["FF"] = 0 };
        var grades = await _context.Notlar.Include(n => n.Ders)
            .Where(n => n.StudentId == student.Id && n.HarfNotu != null).ToListAsync();
        var calculated = grades.Where(n => points.ContainsKey(n.HarfNotu) && n.Ders.Akts > 0).ToList();
        var ects = calculated.Sum(n => n.Ders.Akts);
        var gano = ects == 0 ? 0 : calculated.Sum(n => points[n.HarfNotu] * n.Ders.Akts) / ects;
        return Ok(new { gano = Math.Round(gano, 2), hesaplananAkts = ects });
    }
}

