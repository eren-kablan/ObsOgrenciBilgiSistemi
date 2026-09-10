namespace ObsOgrenciBilgiSistemi.Models
{
    public class Bolum
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;


        public ICollection<Ogrenci> Ogrenciler { get; set; } = new List<Ogrenci>();
        public ICollection<Akademisyen> Akademisyenler { get; set; } = new List<Akademisyen>();
        public int? DanismanAkademisyenId { get; set; }
        public Akademisyen? DanismanAkademisyen { get; set; }
    }
}
