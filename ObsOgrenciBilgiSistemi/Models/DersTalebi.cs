namespace ObsOgrenciBilgiSistemi.Models;

public class DersTalebi
{
    public int Id { get; set; }
    public int DersId { get; set; }
    public Ders Ders { get; set; } = null!;
    public int AkademisyenId { get; set; }
    public Akademisyen Akademisyen { get; set; } = null!;
    public string AkademisyenEmail { get; set; } = string.Empty;
    public string Durum { get; set; } = "Bekliyor";
    public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? KararTarihi { get; set; }
    public string? KararVerenEmail { get; set; }
}
