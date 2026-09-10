using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace ObsOgrenciBilgiSistemi.Features.Auth.Commands
{
    public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ActivateAccountCommandHandler(
            AppDbContext context,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ActivateAccountDto;

            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.Password))
                return false;

            var tokenHash = HashToken(dto.Token);
            var ogrenci = await _context.Ogrenciler
                .FirstOrDefaultAsync(x => x.ActivationTokenHash == tokenHash &&
                                          x.ActivationTokenExpiresAt > DateTime.UtcNow, cancellationToken);

            if (ogrenci == null) return false;

            var user = await _userManager.FindByEmailAsync(ogrenci.Email);

            if (user == null)
            {
                if (string.IsNullOrWhiteSpace(ogrenci.Email))
                {
                    throw new Exception("Bu öğrencinin veritabanında (Ogrenciler tablosu) bir E-Posta adresi yok! E-Posta olmadan kullanıcı oluşturulamaz.");
                }
                user = new AppUser
                {
                    Ad = ogrenci.Adi,
                    Soyadi = ogrenci.Soyadi,
                    UserName = ogrenci.Email,
                    Email = ogrenci.Email,
                    EmailConfirmed = true,
                    IsFirstLogin = false
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Kullanıcı oluşturulamadı: {errors}");
                }

                await _userManager.AddToRoleAsync(user, "Student");
            }
            else
            {
                // Kullanıcı zaten varsa şifresini güvenle sıfırla
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                if (!resetResult.Succeeded) return false;

                // Hesabı güncelleyip aktif hale getir
                user.EmailConfirmed = true;
                user.IsFirstLogin = false;
                await _userManager.UpdateAsync(user);
            }

            ogrenci.IsActive = true;
            ogrenci.ActivationToken = null;
            ogrenci.ActivationTokenHash = null;
            ogrenci.ActivationTokenExpiresAt = null;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
