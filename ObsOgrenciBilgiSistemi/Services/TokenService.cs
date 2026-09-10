using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ObsOgrenciBilgiSistemi.Models; 

namespace ObsOgrenciBilgiSistemi.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(AppUser user, IList<string> roles)
        {
            var email = user.Email ?? user.UserName
                ?? throw new InvalidOperationException("Token oluşturmak için kullanıcının e-posta adresi gereklidir.");
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key yapılandırması bulunamadı.");

            if (!double.TryParse(_configuration["Jwt:ExpireDays"], out var expireDays) || expireDays <= 0)
            {
                throw new InvalidOperationException("Jwt:ExpireDays pozitif bir sayı olmalıdır.");
            }

            var claims = new List<Claim>
            {
                
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, user.Ad)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(expireDays),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            
            return tokenHandler.WriteToken(token); 
        }
    }
}
