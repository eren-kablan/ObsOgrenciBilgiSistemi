namespace ObsOgrenciBilgiSistemi.DTOs
{
    public class TopluNotGirisDto
    {
        public int DersId { get; set; }
        public List<OgrenciNotItemDto> Notlar { get; set; } = new();
    }

    public class OgrenciNotItemDto
    {
        public int OgrenciId { get; set; }
        public string OgrenciNumarasi { get; set; } = string.Empty;
        public decimal? Vize { get; set; }
        public decimal? Final { get; set; }
    }
}