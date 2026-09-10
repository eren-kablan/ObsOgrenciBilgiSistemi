using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Controllers;

[ApiController, Route("api/advisors"), Authorize(Roles = "Admin")]
public class AdvisorsController : ControllerBase
{
    private readonly AppDbContext _context;
    public AdvisorsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Bolumler.AsNoTracking()
        .OrderBy(department => department.Adi)
        .Select(department => new
        {
            department.Id,
            BolumAdi = department.Adi,
            department.DanismanAkademisyenId,
            DanismanAdi = department.DanismanAkademisyen == null ? null :
                department.DanismanAkademisyen.Unvani + " " + department.DanismanAkademisyen.Adi + " " + department.DanismanAkademisyen.Soyadi,
            Akademisyenler = department.Akademisyenler.OrderBy(lecturer => lecturer.Adi).Select(lecturer => new
            {
                lecturer.Id, lecturer.Adi, lecturer.Soyadi, lecturer.Unvani, lecturer.Email
            })
        }).ToListAsync());

    public record AssignAdvisorRequest(int LecturerId);

    [HttpPut("department/{departmentId:int}")]
    public async Task<IActionResult> Assign(int departmentId, AssignAdvisorRequest request)
    {
        var department = await _context.Bolumler.FindAsync(departmentId);
        if (department == null) return NotFound(new { message = "Bölüm bulunamadı." });
        bool belongsToDepartment = await _context.Akademisyenler.AnyAsync(lecturer =>
            lecturer.Id == request.LecturerId && lecturer.BolumId == departmentId);
        if (!belongsToDepartment) return BadRequest(new { message = "Danışman yalnızca aynı bölümün akademisyenleri arasından seçilebilir." });
        department.DanismanAkademisyenId = request.LecturerId;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Bölüm danışmanı başarıyla atandı." });
    }
}
