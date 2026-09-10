namespace ObsOgrenciBilgiSistemi.Models
{
    public class Devamsizlik
    {
        public int Id { get; set; }

        
        public int StudentId { get; set; }
        public Ogrenci Student { get; set; }

        
        public int DersId { get; set; }
        public Ders Ders { get; set; }

        public int DevamsizlikHaftasi { get; set; } 
        public bool Durum { get; set; } // true = geldi devamlı false = gelmedi devamsız
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}
