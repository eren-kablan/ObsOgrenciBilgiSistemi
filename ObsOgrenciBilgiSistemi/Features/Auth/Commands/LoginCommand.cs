using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Models;
using ObsOgrenciBilgiSistemi.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ObsOgrenciBilgiSistemi.Features.Auth.Commands
{
    public class LoginCommand : IRequest<LoginResponseDto>
    {
        public LoginDto LoginDto { get; set; } = null!;
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly AppDbContext _context;

        public LoginCommandHandler(UserManager<AppUser> userManager, TokenService tokenService, AppDbContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Kullanıcı sorgusu
            var dto = request.LoginDto;
            var loginKey = dto.Email?.Trim() ?? string.Empty;

            var user = await _userManager.FindByEmailAsync(loginKey);

            if (user == null) user = await _userManager.FindByNameAsync(loginKey);

            if (user == null)
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginKey || u.UserName == loginKey, cancellationToken);
            }

            if (user == null && !loginKey.Contains("@"))
            {
                var generatedStudentEmail = $"{loginKey}@ogr.duzce.edu.tr";
                user = await _userManager.FindByEmailAsync(generatedStudentEmail)
                    ?? await _context.Users.FirstOrDefaultAsync(u => u.Email == generatedStudentEmail, cancellationToken);
            }

            if (user == null)
                throw new UnauthorizedAccessException();

            if (user.IsFirstLogin && user.TemporaryPasswordExpiresAt is { } expiry && expiry <= DateTime.UtcNow)
                throw new UnauthorizedAccessException();

            // Şifre ve token kontrolü
            var checkPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!checkPassword)
                throw new UnauthorizedAccessException();

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException();

            // Login yanıtı
            var response = new LoginResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? "Student",
                IsFirstLogin = user.IsFirstLogin,
                Ad = user.Ad,
                Soyad = user.Soyadi
            };

            // Akademisyen profil bilgileri
            if (roles.Contains("Lecturer") || roles.Contains("Akademisyen"))
            {
                var lecturer = await _context.Akademisyenler
                    .FirstOrDefaultAsync(x => x.Email == user.Email, cancellationToken);

                if (lecturer != null)
                {
                    response.Unvan = lecturer.Unvani;
                    response.Ad = lecturer.Adi;
                    response.Soyad = lecturer.Soyadi;
                    response.Fakulte = "Mühendislik Fakültesi";
                    response.Bolum = "Bilgisayar Mühendisliği";
                }
            }

            return response;
        }
    }
}
