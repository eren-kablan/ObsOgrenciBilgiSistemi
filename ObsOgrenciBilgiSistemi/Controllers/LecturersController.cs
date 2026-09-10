using Microsoft.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ObsOgrenciBilgiSistemi.Features.Lecturers.Commands;
using ObsOgrenciBilgiSistemi.Features.Lecturers.Queries;

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LecturersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LecturersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. TÜM AKADEMİSYENLERİ VEYA BÖLÜME GÖRE LİSTELEME
        // Akademisyen listeleme
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? bolumId)
        {
            var query = new GetAllLecturersQuery { BolumId = bolumId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 2. ID İLE AKADEMİSYEN GETİRME (GÜNCELLEME EKRANI İÇİN)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetLecturerByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            if (result == null)
            {
                return NotFound(new { Message = "Akademisyen bulunamadı." });
            }
            return Ok(result);
        }

        // 3. AKADEMİSYEN EKLEME
        // Akademisyen kayıt işlemi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateLecturerCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                Message = "Akademisyen başarıyla eklendi ve kurumsal maili oluşturuldu!",
                Id = result.Id,
                Email = result.Email,
                TempPassword = result.TempPassword
            });
        }

        // 4. AKADEMİSYEN GÜNCELLEME (ÜNVAN, AD, SOYAD, BÖLÜM VS.)
        // Akademisyen güncelleme ve silme
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLecturerCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { Message = "ID uyuşmazlığı!" });
            }

            var result = await _mediator.Send(command);
            if (!result)
            {
                return NotFound(new { Message = "Güncellenecek akademisyen bulunamadı." });
            }

            return Ok(new { Message = "Akademisyen bilgileri başarıyla güncellendi!" });
        }

        // 5. AKADEMİSYEN SİLME
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteLecturerCommand { Id = id };
            var result = await _mediator.Send(command);
            if (!result)
            {
                return NotFound(new { Message = "Silinecek akademisyen bulunamadı." });
            }

            return Ok(new { Message = "Akademisyen başarıyla silindi." });
        }
    }
}

