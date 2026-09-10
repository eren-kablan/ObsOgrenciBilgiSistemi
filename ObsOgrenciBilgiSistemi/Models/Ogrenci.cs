using System.Text.Json.Serialization;
namespace ObsOgrenciBilgiSistemi.Models
{
    public class Ogrenci
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OgrenciNumarasi { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; } = null!;
        public ICollection<OgrenciDers> OgrenciDersler { get; set; } = new List<OgrenciDers>();
        [JsonIgnore]
        public string? ActivationToken { get; set; }
        [JsonIgnore]
        public string? ActivationTokenHash { get; set; }
        public DateTime? ActivationTokenExpiresAt { get; set; }
        public DateTime? ActivationEmailSentAt { get; set; }
        public bool IsActive { get; set; } = false;

        [JsonIgnore]
        public string? Sifre { get; set; }
        public int Sinif { get; set; }
    }
}

