using System.Text.Json.Serialization;
namespace ObsOgrenciBilgiSistemi.Models

{
    public class Akademisyen
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string Unvani { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; } = null!;
        // parola burada tutulmuyo aspde hashli tutulur
        
        [JsonIgnore]
        public string? Sifre { get; set; }
    }
}

