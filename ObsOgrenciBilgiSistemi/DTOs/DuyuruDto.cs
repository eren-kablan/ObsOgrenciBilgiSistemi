namespace ObsOgrenciBilgiSistemi.DTOs;

public class DuyuruDto
{
    public string Baslik { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;
    public string HedefRol { get; set; } = "Student";
    public bool Yayinda { get; set; } = true;
}
