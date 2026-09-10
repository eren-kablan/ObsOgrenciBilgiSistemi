using MediatR;
using Microsoft.AspNetCore.Identity;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Entities;
using ObsOgrenciBilgiSistemi.Models;
using System.Security.Cryptography;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class CreateLecturerCommandHandler : IRequestHandler<CreateLecturerCommand, CreateLecturerResponseDto>
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;

        public CreateLecturerCommandHandler(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        public async Task<CreateLecturerResponseDto> Handle(CreateLecturerCommand request, CancellationToken cancellationToken)
        {
            // epostayı kurumsal sekilde olusturma türkce karakter varsa onları dönüstürme islemleri  ğ->g , ...
            string ad = request.Adi.Trim().ToLower()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            string soyad = request.Soyadi.Trim().ToLower()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            string generatedEmail = string.IsNullOrWhiteSpace(request.Email)
                ? $"{ad}.{soyad}@duzce.edu.tr"
                : request.Email;

            int counter = 1;
            string baseEmailName = $"{ad}.{soyad}";

            while (await _userManager.FindByEmailAsync(generatedEmail) != null)
            {
                generatedEmail = $"{baseEmailName}{counter}@duzce.edu.tr";
                counter++;
            }

            string tempPassword = CreateTemporaryPassword();

            // identity kullanıcısı
            var appUser = new AppUser
            {
                UserName = generatedEmail,
                Email = generatedEmail,
                EmailConfirmed = true,
                Ad = request.Adi,
                Soyadi = request.Soyadi,
                IsFirstLogin = true,
                TemporaryPasswordExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            var identityResult = await _userManager.CreateAsync(appUser, tempPassword);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                throw new Exception($"Kullanıcı oluşturulurken hata oluştu: {errors}");
            }

            // akademisyen rol
            const string roleName = "Lecturer";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new AppRole { Name = roleName });
            }
            await _userManager.AddToRoleAsync(appUser, roleName);

            // akademisyen kaydi
            var lecturer = new Akademisyen
            {
                Adi = request.Adi,
                Soyadi = request.Soyadi,
                Unvani = request.Unvani,
                BolumId = request.BolumId,
                Email = generatedEmail,
                // sifre hash altinda tutulur.
                Sifre = null
            };

            _context.Akademisyenler.Add(lecturer);
            await _context.SaveChangesAsync(cancellationToken);

            // giris e-posta
            string emailSubject = "Düzce Üniversitesi OBS Akademisyen Giriş Bilgileriniz";
            string emailBody = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f8fafc;'>
                    <div style='max-width: 500px; margin: 0 auto; background: #ffffff; padding: 30px; border-radius: 12px; border: 1px solid #e2e8f0;'>
                        <h2 style='color: #1d4ed8; margin-top: 0;'>Sayın {request.Unvani} {request.Adi} {request.Soyadi},</h2>
                        <p style='color: #475569;'>Düzce Üniversitesi Öğrenci Bilgi Sistemi akademisyen hesabınız oluşturulmuştur.</p>
                        <p style='color: #475569;'><strong>Kullanıcı Adı:</strong> {generatedEmail}</p>
                        <p style='color: #475569;'><strong>Geçici Şifreniz:</strong> <span style='font-family: monospace; background: #e2e8f0; padding: 4px 8px; border-radius: 4px;'>{tempPassword}</span></p>
                        <br/>
                        <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Sisteme ilk girişinizde şifrenizi değiştirmeniz önerilir.</p>
                    </div>
                </div>";

            await _emailService.SendEmailAsync(generatedEmail, emailSubject, emailBody);

            return new CreateLecturerResponseDto
            {
                Id = lecturer.Id,
                Email = lecturer.Email,
                TempPassword = tempPassword
            };
        }

        private static string CreateTemporaryPassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(16);
            var value = string.Concat(bytes.Select(b => alphabet[b % alphabet.Length]));
            return $"Du.{value}1!";
        }
    }
}
