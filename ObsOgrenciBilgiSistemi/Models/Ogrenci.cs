using System.Text.Json.Serialization;
namespace ObsOgrenciBilgiSistemi.Models
{
    public class Ogrenci
    {
        public int Id { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public string Email { get; set; }
        public string OgrenciNumarasi { get; set; }
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; }
        public ICollection<OgrenciDers> OgrenciDersler { get; set; }
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

