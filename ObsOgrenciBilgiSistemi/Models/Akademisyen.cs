using System.Text.Json.Serialization;
namespace ObsOgrenciBilgiSistemi.Models

{
    public class Akademisyen
    {
        public int Id { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public string Unvani { get; set; }
        public string Email { get; set; }       
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; }
        // parola burada tutulmuyo aspde hashli tutulur
        
        [JsonIgnore]
        public string? Sifre { get; set; }
    }
}

