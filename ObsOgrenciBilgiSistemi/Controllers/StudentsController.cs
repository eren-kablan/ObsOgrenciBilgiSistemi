using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ObsOgrenciBilgiSistemi.Features.Students.Commands;
using ObsOgrenciBilgiSistemi.Features.Students.Queries;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StudentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Öğrenci kayıt işlemi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStudentCommand command)
        {
            var studentId = await _mediator.Send(command);
            return Ok(new { Message = "Öğrenci başarıyla eklendi ve kurumsal maili oluşturuldu!", Id = studentId });
        }

        // Öğrenci listeleme ve filtreleme
        [HttpGet]
        [Authorize(Roles = "Admin,Lecturer,Akademisyen")]
        public async Task<IActionResult> GetAll([FromQuery] int? dersId, [FromQuery] int? bolumId)
        {
            var query = new GetAllStudentsQuery { DersId = dersId, BolumId = bolumId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // Angular tarafındaki ID ile öğrenci getirme araması için eklendi
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetStudentByIdQuery { Id = id };
            var student = await _mediator.Send(query);
            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }
            return Ok(student);
        }

        // Öğrenci profil sorguları
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            if (!User.IsInRole("Admin") && !string.Equals(User.FindFirstValue(ClaimTypes.Email), email, StringComparison.OrdinalIgnoreCase)) return Forbid();
            var query = new GetStudentByEmailQuery { Email = email };
            var student = await _mediator.Send(query);
            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }
            return Ok(student);
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            if (!User.IsInRole("Admin") && !string.Equals(User.FindFirstValue(ClaimTypes.Email), email, StringComparison.OrdinalIgnoreCase)) return Forbid();
            var query = new GetStudentByEmailQuery { Email = email };
            var student = await _mediator.Send(query);
            if (student == null)
            {
                return NotFound(new { message = "Öğrenci bulunamadı." });
            }
            return Ok(student);
        }

        // Öğrenci güncelleme ve silme
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID uyuşmazlığı!");
            }

            var result = await _mediator.Send(command);
            if (!result)
            {
                return NotFound("Güncellenecek öğrenci bulunamadı.");
            }

            return Ok(new { Message = "Öğrenci başarıyla güncellendi ve mail adresi yenilendi!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteStudentCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound("Silinecek öğrenci bulunamadı.");
            }

            return Ok(new { Message = "Öğrenci sistemden başarıyla silindi." });
        }
    }
}

