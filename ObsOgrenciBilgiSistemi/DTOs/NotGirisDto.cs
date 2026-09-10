namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class NotGirisDto
    {
        public string OgrenciNo { get; set; } = string.Empty;
        public string DersKodu { get; set; } = string.Empty;
        public decimal? Vize { get; set; }
        public decimal? Final { get; set; }
    }
}