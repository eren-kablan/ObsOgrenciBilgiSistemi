namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class DevamsizlikGirisDto
    {
        public string StudentId { get; set; } = string.Empty;
        public int DersId { get; set; }
        public int DevamsizlikHaftasi { get; set; }
        public bool Durum { get; set; }
    }
}
