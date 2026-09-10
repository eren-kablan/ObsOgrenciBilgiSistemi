namespace ObsOgrenciBilgiSistemi.Models;

public class Duyuru
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public string HedefRol { get; set; } = "Student";
    public bool Yayinda { get; set; }
    public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? YayinTarihi { get; set; }
    public string OlusturanEmail { get; set; } = string.Empty;
}
