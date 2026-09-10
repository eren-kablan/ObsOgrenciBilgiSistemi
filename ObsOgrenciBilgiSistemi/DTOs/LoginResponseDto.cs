namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsFirstLogin { get; set; }
        public string Unvan { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Bolum { get; set; } = string.Empty;
        public string Fakulte { get; set; } = string.Empty;
    }
}