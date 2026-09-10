using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;
    public NotificationsController(AppDbContext context) => _context = context;

    // Kullanıcının bildirimleri
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        string email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var notifications = await _context.Bildirimler
            .Where(notification => notification.AliciEmail == email)
            .OrderByDescending(notification => notification.OlusturulmaTarihi)
            .Take(50)
            .Select(notification => new
            {
                notification.Id,
                notification.Baslik,
                notification.Mesaj,
                notification.Okundu,
                notification.OlusturulmaTarihi
            })
            .ToListAsync(cancellationToken);
        return Ok(notifications);
    }

    // Bildirimi okundu işaretleme
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        string email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var notification = await _context.Bildirimler
            .FirstOrDefaultAsync(item => item.Id == id && item.AliciEmail == email, cancellationToken);
        if (notification is null) return NotFound();
        notification.Okundu = true;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // Kullanıcı yalnızca kendisine ait bildirimi silebilir.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMine(int id, CancellationToken cancellationToken)
    {
        string email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var notification = await _context.Bildirimler
            .FirstOrDefaultAsync(item => item.Id == id && item.AliciEmail == email, cancellationToken);
        if (notification is null) return NotFound();
        _context.Bildirimler.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
