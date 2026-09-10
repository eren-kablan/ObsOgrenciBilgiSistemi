namespace ObsOgrenciBilgiSistemi.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Not
{
    public int Id { get; set; }

  
    [ForeignKey("Student")]
    public int StudentId { get; set; } 
    public Ogrenci Student { get; set; } = null!;

    [ForeignKey("Ders")]
    public int DersId { get; set; }
    public Ders Ders { get; set; } = null!;
    [Column(TypeName = "decimal(5,2)")]
    public decimal? Vize { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Final { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Ortalama { get; set; }

    public string HarfNotu { get; set; } = string.Empty;
}
