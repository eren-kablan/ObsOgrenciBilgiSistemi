using ObsOgrenciBilgiSistemi.Enums;

namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string DersKodu { get; set; }
        public string Adi { get; set; }
        public int Kredi { get; set; }
        public int Akts { get; set; }
        public int BolumId { get; set; }
        public string BolumAdi { get; set; }
        public string? AkademisyenEmail { get; set; }
        public string? AkademisyenAdi { get; set; }
        public int Sinif { get; set; }          
        public DonemTipi Donem { get; set; }    
    }
}

