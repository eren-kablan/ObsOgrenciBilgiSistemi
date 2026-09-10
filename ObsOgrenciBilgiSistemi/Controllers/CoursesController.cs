using MediatR;
using Microsoft.AspNetCore.Mvc;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Features.Courses.Commands;
using ObsOgrenciBilgiSistemi.Features.Courses.Queries;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AppDbContext _context;
        private readonly DuzceCurriculumImportService _curriculumImportService;

        public CoursesController(
            IMediator mediator,
            AppDbContext context,
            DuzceCurriculumImportService curriculumImportService)
        {
            _mediator = mediator;
            _context = context;
            _curriculumImportService = curriculumImportService;
        }

        // Ders listeleme
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllCoursesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // Öğrencinin yalnızca kendi bölüm, sınıf ve dönemindeki dersleri
        [HttpGet("registration-options")]
        [Authorize(Roles = "Student,ogrenci,Öğrenci")]
        public async Task<IActionResult> GetRegistrationOptions([FromQuery] int term)
        {
            if (term is < 1 or > 2)
                return BadRequest(new { message = "Dönem bilgisi geçersiz." });

            var email = User.FindFirstValue(ClaimTypes.Email);
            var student = await _context.Ogrenciler.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Email == email);
            if (student == null)
                return NotFound(new { message = "Öğrenci bulunamadı." });

            var courses = await _context.Dersler.AsNoTracking()
                .Where(course => course.BolumId == student.BolumId &&
                    course.Sinif == student.Sinif && (int)course.Donem == term)
                .OrderBy(course => course.DersKodu)
                .Select(course => new CourseDto
                {
                    Id = course.Id,
                    DersKodu = course.DersKodu,
                    Adi = course.Adi,
                    Kredi = course.Kredi,
                    Akts = course.Akts,
                    BolumId = course.BolumId,
                    BolumAdi = course.Bolum.Adi,
                    AkademisyenEmail = course.AkademisyenEmail,
                    AkademisyenAdi = course.Akademisyen == null
                        ? null
                        : course.Akademisyen.Unvani + " " + course.Akademisyen.Adi + " " + course.Akademisyen.Soyadi,
                    Sinif = course.Sinif,
                    Donem = course.Donem
                })
                .ToListAsync();

            return Ok(courses);
        }

        // Ders oluşturma
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCourseCommand command)
        {
            int groupCourseCount = await _context.Dersler.CountAsync(course => course.BolumId == command.BolumId &&
                course.Sinif == command.Sinif && course.Donem == command.Donem);
            if (groupCourseCount >= 5)
                return Conflict(new { message = "Bu bölüm, sınıf ve dönem için en fazla 5 ders tanımlanabilir." });
            var id = await _mediator.Send(command);
            return Ok(new { Message = "Ders başarıyla eklendi!", Id = id });
        }

        // Derse akademisyen atama
        [HttpPost("AkademisyenAta")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AkademisyenAta([FromBody] DersAkademisyenAtaDto dto)
        {
            var command = new AssignLecturerToCourseCommand
            {
                DersId = dto.DersId,
                AkademisyenEmail = dto.AkademisyenEmail
            };

            var result = await _mediator.Send(command);
            if (!result) return NotFound("Ders bulunamadı.");

            return Ok(new { message = "Akademisyen derse başarıyla atandı." });
        }

        [HttpPost("mufredat-ice-aktar")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportCurricula(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _curriculumImportService.ImportAllAsync(cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = exception.Message });
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = "Üniversitenin EBS servisine şu anda ulaşılamıyor. Lütfen daha sonra yeniden deneyin."
                });
            }
        }

        // Ders ve bağlı kayıtları silme
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            var deleted = await strategy.ExecuteAsync(async () =>
            {
                var ders = await _context.Dersler.FirstOrDefaultAsync(item => item.Id == id);
                if (ders == null) return false;
                
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await _context.DersKayitTalepDersleri
                    .Where(item => item.DersId == id)
                    .ExecuteDeleteAsync();
                await _context.Notlar
                    .Where(item => item.DersId == id)
                    .ExecuteDeleteAsync();
                await _context.Devamsizliklar
                    .Where(item => item.DersId == id)
                    .ExecuteDeleteAsync();
                await _context.OgrenciDersler
                    .Where(item => item.DersId == id)
                    .ExecuteDeleteAsync();
                await _context.DersTalepleri
                    .Where(item => item.DersId == id)
                    .ExecuteDeleteAsync();


                _context.Dersler.Remove(ders);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            });

            if (!deleted) return NotFound("Ders bulunamadı.");
            return Ok(new { message = "Ders ve bağlı kayıtları silindi." });
        }
    }
}
