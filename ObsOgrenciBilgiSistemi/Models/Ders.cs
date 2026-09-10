using ObsOgrenciBilgiSistemi.Enums;

namespace ObsOgrenciBilgiSistemi.Models
{
    public class Ders
    {
        public int Id { get; set; }
        public string DersKodu { get; set; } 
        public string Adi { get; set; }     
        public int Kredi { get; set; }          
        public int BolumId { get; set; }
        public Bolum Bolum { get; set; }
        public ICollection<OgrenciDers> OgrenciDersler { get; set; }
        public int? AkademisyenId { get; set; }
        public Akademisyen Akademisyen { get; set; }
        public string? AkademisyenEmail { get; set; } 
        public int Sinif { get; set; }
        public DonemTipi Donem { get; set; }
        public int Akts { get; set; }

    }
}