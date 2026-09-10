namespace ObsOgrenciBilgiSistemi.Models;

public class Bildirim
{
    public int Id { get; set; }
    public string AliciEmail { get; set; } = string.Empty;
    public string Baslik { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public bool Okundu { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
}
