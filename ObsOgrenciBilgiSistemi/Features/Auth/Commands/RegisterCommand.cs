using MediatR;
using Microsoft.AspNetCore.Identity;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Models; 
using ObsOgrenciBilgiSistemi.Entities; 

namespace ObsOgrenciBilgiSistemi.Features.Auth.Commands
{
    public class RegisterCommand : IRequest<string>
    {
        public RegisterDto RegisterDto { get; set; } = null!;
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
    {
        private readonly UserManager<AppUser> _userManager;
        
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;

        public RegisterCommandHandler(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.RegisterDto;

            var generatedEmail = $"{dto.Ad.ToLower().Trim()}.{dto.Soyadi.ToLower().Trim()}@ogr.duzce.edu.tr"
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            var user = new AppUser
            {
                UserName = generatedEmail,
                Email = generatedEmail,
                Ad = dto.Ad,
                Soyadi = dto.Soyadi
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return string.Join(", ", result.Errors.Select(e => e.Description));

            if (!await _roleManager.RoleExistsAsync(dto.Rol))
            {
                
                await _roleManager.CreateAsync(new AppRole { Name = dto.Rol });
            }

            await _userManager.AddToRoleAsync(user, dto.Rol);

            if (dto.Rol.Equals("Ogrenci", StringComparison.OrdinalIgnoreCase))
            {
                // Rastgele sayı üreticiyi tanımla
                var random = new Random();
               
                var uretilenOgrenciNo = DateTime.Now.Year.ToString() + random.Next(1000, 9999).ToString();

                var yeniOgrenci = new Ogrenci
                {
                    Adi = dto.Ad,
                    Soyadi = dto.Soyadi,
                    Email = generatedEmail,
                    OgrenciNumarasi = uretilenOgrenciNo, 
                    BolumId = 1
                };

                _context.Ogrenciler.Add(yeniOgrenci);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else if (dto.Rol.Equals("Akademisyen", StringComparison.OrdinalIgnoreCase))
            {
                var yeniAkademisyen = new Akademisyen
                {
                    Adi = dto.Ad,
                    Soyadi = dto.Soyadi,
                    Email = generatedEmail,
                    Unvani = "Dr.",
                    BolumId = 1
                   
                };

                _context.Akademisyenler.Add(yeniAkademisyen);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return $"Kayıt işleminiz başarılı : Email adresiniz: {generatedEmail}";
        }
    }
}
