using Microsoft.AspNetCore.Identity;

namespace ObsOgrenciBilgiSistemi.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string Ad { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public bool IsFirstLogin { get; set; } = true;
        public DateTime? TemporaryPasswordExpiresAt { get; set; }
    }
}
