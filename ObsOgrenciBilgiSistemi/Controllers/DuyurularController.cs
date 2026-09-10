using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Models;
using System.Security.Claims;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuyurularController : ControllerBase
{
    private readonly AppDbContext _context;
    public DuyurularController(AppDbContext context) => _context = context;

    // Duyuru listeleme
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IQueryable<Duyuru> query = _context.Set<Duyuru>();
        if (!User.IsInRole("Admin"))
        {
            var role = User.IsInRole("Lecturer") ? "Lecturer" : "Student";
            query = query.Where(x => x.Yayinda && (x.HedefRol == "All" || x.HedefRol == role));
        }
        return Ok(await query.OrderByDescending(x => x.YayinTarihi ?? x.OlusturulmaTarihi).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _context.Set<Duyuru>().FindAsync(id);
        if (item == null) return NotFound(new { message = "Duyuru bulunamadı." });
        if (!User.IsInRole("Admin"))
        {
            var role = User.IsInRole("Lecturer") ? "Lecturer" : "Student";
            if (!item.Yayinda || (item.HedefRol != "All" && item.HedefRol != role)) return NotFound();
        }
        return Ok(item);
    }

    // Duyuru oluşturma
    [HttpPost]
    [Authorize(Roles = "Admin,Lecturer,Akademisyen")]
    public async Task<IActionResult> Create(DuyuruDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Baslik) || string.IsNullOrWhiteSpace(dto.Icerik))
            return BadRequest(new { message = "Başlık ve içerik zorunludur." });
        if (!User.IsInRole("Admin")) dto.HedefRol = "Student";
        if (dto.HedefRol is not ("All" or "Student" or "Lecturer"))
            return BadRequest(new { message = "Geçersiz hedef kitle." });
        var item = new Duyuru { Baslik = dto.Baslik.Trim(), Icerik = dto.Icerik.Trim(), HedefRol = dto.HedefRol, Yayinda = dto.Yayinda, OlusturanEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty, YayinTarihi = dto.Yayinda ? DateTime.UtcNow : null };
        _context.Add(item); await _context.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    // Duyuru güncelleme ve silme
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, DuyuruDto dto)
    {
        var item = await _context.Set<Duyuru>().FindAsync(id);
        if (item == null) return NotFound(new { message = "Duyuru bulunamadı." });
        if (string.IsNullOrWhiteSpace(dto.Baslik) || string.IsNullOrWhiteSpace(dto.Icerik) || dto.HedefRol is not ("All" or "Student" or "Lecturer")) return BadRequest(new { message = "Duyuru bilgileri geçersiz." });
        item.Baslik = dto.Baslik.Trim(); item.Icerik = dto.Icerik.Trim(); item.HedefRol = dto.HedefRol;
        if (!item.Yayinda && dto.Yayinda) item.YayinTarihi = DateTime.UtcNow;
        item.Yayinda = dto.Yayinda; await _context.SaveChangesAsync(); return Ok(item);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Set<Duyuru>().FindAsync(id);
        if (item == null) return NotFound(new { message = "Duyuru bulunamadı." });
        _context.Remove(item); await _context.SaveChangesAsync(); return NoContent();
    }
}
