namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string OgrenciNumarasi { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public string BolumAdi { get; set; } = string.Empty;
        public int Sinif { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
