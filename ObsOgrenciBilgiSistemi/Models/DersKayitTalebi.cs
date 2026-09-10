namespace ObsOgrenciBilgiSistemi.Models;

public enum DersKayitDurumu { Bekliyor = 0, Onaylandi = 1, Reddedildi = 2 }

public class DersKayitTalebi
{
    public int Id { get; set; }
    public int OgrenciId { get; set; }
    public Ogrenci Ogrenci { get; set; } = null!;
    public int DanismanAkademisyenId { get; set; }
    public Akademisyen DanismanAkademisyen { get; set; } = null!;
    public string AkademikYil { get; set; } = string.Empty;
    public int Donem { get; set; }
    public DersKayitDurumu Durum { get; set; } = DersKayitDurumu.Bekliyor;
    public string? RedNedeni { get; set; }
    public DateTime TalepTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? KararTarihi { get; set; }
    public ICollection<DersKayitTalepDersi> Dersler { get; set; } = new List<DersKayitTalepDersi>();
}

public class DersKayitTalepDersi
{
    public int Id { get; set; }
    public int DersKayitTalebiId { get; set; }
    public DersKayitTalebi DersKayitTalebi { get; set; } = null!;
    public int DersId { get; set; }
    public Ders Ders { get; set; } = null!;
}
