using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Services;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Features.Auth.Commands;
using ObsOgrenciBilgiSistemi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;   

namespace ObsOgrenciBilgiSistemi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AuthController(IMediator mediator, UserManager<AppUser> userManager, AppDbContext context, IEmailService emailService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }

        // Kayıt ve giriş işlemleri
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var message = await _mediator.Send(new RegisterCommand { RegisterDto = registerDto });
            if (string.IsNullOrEmpty(message)) return BadRequest("Kayıt olma işlemi başarısız.");
            return Ok(message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var response = await _mediator.Send(new LoginCommand { LoginDto = loginDto });
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "E-posta adresi veya şifre hatalı." });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Giriş işlemi tamamlanamadı. Lütfen tekrar deneyin." });
            }
        }

        // Hesap aktivasyonu
        [HttpPost("ActivateAccount")]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivateAccountDto activateAccountDto)
        {
            try
            {
                var isSuccess = await _mediator.Send(new ActivateAccountCommand { ActivateAccountDto = activateAccountDto });
                if (!isSuccess)
                    return BadRequest(new { message = "Geçersiz veya kullanılmış aktivasyon bağlantısı." });

                return Ok(new { message = "Hesap başarıyla aktifleştirildi." });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Aktivasyon tamamlanamadı. Bağlantının süresi dolmuş olabilir." });
            }
        }

        // Şifre işlemleri
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = email == null ? null : await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Mevcut ve yeni şifre zorunludur." });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = $"Şifre kriterlere uygun değil: {errors}" });
            }

            user.IsFirstLogin = false;
            user.TemporaryPasswordExpiresAt = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Şifre başarıyla güncellendi." });
        }

        // Aktivasyon e-postası
        [HttpPost("activation/resend/{studentId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResendActivation(int studentId)
        {
            var student = await _context.Ogrenciler.FindAsync(studentId);
            if (student == null) return NotFound(new { message = "Öğrenci bulunamadı." });
            if (student.IsActive) return BadRequest(new { message = "Bu öğrenci hesabı zaten aktiftir." });
            if (student.ActivationEmailSentAt > DateTime.UtcNow.AddMinutes(-2))
                return BadRequest(new { message = "Güvenlik nedeniyle yeni aktivasyon e-postası için iki dakika bekleyin." });

            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            student.ActivationToken = null;
            student.ActivationTokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
            student.ActivationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            student.ActivationEmailSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _emailService.SendActivationEmailAsync(student.Email, $"{student.Adi} {student.Soyadi}", rawToken);
            return Ok(new { message = "Aktivasyon e-postası yeniden gönderildi." });
        }
    }
}

