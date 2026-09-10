namespace ObsOgrenciBilgiSistemi.Models
{
    public class Bolum
    {
        public int Id { get; set; }
        public string Adi { get; set; } 


        public ICollection<Ogrenci> Ogrenciler { get; set; }
        public ICollection<Akademisyen> Akademisyenler { get; set; }
        public int? DanismanAkademisyenId { get; set; }
        public Akademisyen? DanismanAkademisyen { get; set; }
    }
}
