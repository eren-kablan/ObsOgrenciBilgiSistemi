using MediatR;
using Microsoft.AspNetCore.Mvc;
using ObsOgrenciBilgiSistemi.Features.StudentCourses.Commands;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentCoursesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly AppDbContext _context;

        public StudentCoursesController(IMediator mediator, AppDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignCourse([FromBody] AssignCourseToStudentCommand command)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            bool studentExists = await _context.Ogrenciler.AnyAsync(student => student.Id == command.OgrenciId);
            if (!studentExists) return NotFound(new { message = "Öğrenci bulunamadı." });

            try
            {
                var id = await _mediator.Send(command);
                return Ok(new { Message = "Öğrenciye ders başarıyla atandı!", Id = id });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
