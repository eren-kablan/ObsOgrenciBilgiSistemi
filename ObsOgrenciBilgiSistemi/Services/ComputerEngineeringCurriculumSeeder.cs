using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Enums;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Services;

public static class ComputerEngineeringCurriculumSeeder
{
   
    private record CourseSeed(string Code, string Name, int Year, DonemTipi Term, int Credit, int Ects);

    public static async Task SeedAsync(AppDbContext context)
    {
        
        var department = await context.Bolumler.FirstOrDefaultAsync(b => b.Adi.Contains("Bilgisayar"));
        if (department is null) return;

       
        var courses = new[] {
            new CourseSeed("AIB101", "Atatürk İlkeleri ve İnkılap Tarihi I", 1, DonemTipi.Guz, 2, 2), new CourseSeed("BM103", "Bilgisayar Mühendisliğine Giriş", 1, DonemTipi.Guz, 2, 2), new CourseSeed("BM107", "Elektrik Devre Temelleri", 1, DonemTipi.Guz, 3, 4), new CourseSeed("BM111", "Algoritmalar ve Programlama I", 1, DonemTipi.Guz, 3, 4), new CourseSeed("BM115", "Bilişim Teknolojileri", 1, DonemTipi.Guz, 2, 2), new CourseSeed("FIZ111", "Fizik I", 1, DonemTipi.Guz, 4, 6), new CourseSeed("ING101", "İngilizce I", 1, DonemTipi.Guz, 2, 2), new CourseSeed("MAT111", "Matematik I", 1, DonemTipi.Guz, 6, 6), new CourseSeed("TDB121", "Türk Dili I", 1, DonemTipi.Guz, 2, 2),
            new CourseSeed("AIB102", "Atatürk İlkeleri ve İnkılap Tarihi II", 1, DonemTipi.Bahar, 2, 2), new CourseSeed("BM106", "Olasılık ve İstatistik", 1, DonemTipi.Bahar, 3, 4), new CourseSeed("BM112", "Algoritmalar ve Programlama II", 1, DonemTipi.Bahar, 3, 4), new CourseSeed("BM114", "Web Teknolojileri", 1, DonemTipi.Bahar, 3, 4), new CourseSeed("FIZ112", "Fizik II", 1, DonemTipi.Bahar, 4, 6), new CourseSeed("ING102", "İngilizce II", 1, DonemTipi.Bahar, 2, 2), new CourseSeed("KRP102", "Kariyer Planlama", 1, DonemTipi.Bahar, 1, 2), new CourseSeed("MAT112", "Matematik II", 1, DonemTipi.Bahar, 6, 6), new CourseSeed("TDB122", "Türk Dili II", 1, DonemTipi.Bahar, 2, 2),
            new CourseSeed("BM203", "Elektronik", 2, DonemTipi.Guz, 4, 5), new CourseSeed("BM213", "Lineer Cebir", 2, DonemTipi.Guz, 3, 3), new CourseSeed("BM217", "Ayrık İşlemsel Yapılar", 2, DonemTipi.Guz, 3, 3), new CourseSeed("BM221", "Diferansiyel Denklemler", 2, DonemTipi.Guz, 4, 4), new CourseSeed("BM223", "Mesleki İngilizce I", 2, DonemTipi.Guz, 3, 4), new CourseSeed("BM225", "Nesneye Dayalı Programlama", 2, DonemTipi.Guz, 4, 5), new CourseSeed("BM229", "Sayısal Analiz", 2, DonemTipi.Guz, 3, 3),
            new CourseSeed("BM204", "Bilgisayar Organizasyonu", 2, DonemTipi.Bahar, 3, 5), new CourseSeed("BM206", "Sayısal Elektronik", 2, DonemTipi.Bahar, 4, 5), new CourseSeed("BM208", "Nesneye Dayalı Analiz ve Tasarım", 2, DonemTipi.Bahar, 3, 4), new CourseSeed("BM210", "Programlama Dillerinin Prensipleri", 2, DonemTipi.Bahar, 3, 4), new CourseSeed("BM214", "Veri Yapıları", 2, DonemTipi.Bahar, 3, 5), new CourseSeed("BM216", "Mesleki İngilizce II", 2, DonemTipi.Bahar, 3, 4),
            new CourseSeed("BM301", "Biçimsel Diller ve Soyut Makinalar", 3, DonemTipi.Guz, 3, 4), new CourseSeed("BM303", "İşaretler ve Sistemler", 3, DonemTipi.Guz, 3, 4), new CourseSeed("BM305", "İşletim Sistemleri", 3, DonemTipi.Guz, 3, 5), new CourseSeed("BM307", "Bilgisayar Ağları I", 3, DonemTipi.Guz, 3, 5), new CourseSeed("BM309", "Veritabanı Yönetim Sistemleri", 3, DonemTipi.Guz, 4, 6), new CourseSeed("BM397", "Yaz Dönemi Stajı I", 3, DonemTipi.Guz, 1, 3),
            new CourseSeed("BM302", "Bilgisayar Ağları II", 3, DonemTipi.Bahar, 4, 6), new CourseSeed("BM304", "Mikroişlemciler", 3, DonemTipi.Bahar, 4, 6), new CourseSeed("BM306", "Sistem Programlama", 3, DonemTipi.Bahar, 2, 5), new CourseSeed("BM308", "Web Programlama", 3, DonemTipi.Bahar, 4, 6), new CourseSeed("BM310", "Yazılım Mühendisliği", 3, DonemTipi.Bahar, 3, 4),
            new CourseSeed("BM401", "Bilgisayar Mühendisliği Proje Tasarımı", 4, DonemTipi.Guz, 2, 3), new CourseSeed("BM497", "Yaz Dönemi Stajı II", 4, DonemTipi.Guz, 1, 2), new CourseSeed("BM498", "Mezuniyet Tezi", 4, DonemTipi.Bahar, 2, 5), new CourseSeed("BM404", "İşletmede Mesleki Eğitim", 4, DonemTipi.Bahar, 5, 25), new CourseSeed("BM437", "Yapay Zeka", 4, DonemTipi.Guz, 3, 5), new CourseSeed("BM435", "Veri Madenciliği", 4, DonemTipi.Guz, 3, 5), new CourseSeed("BM441", "Bulut Bilişim", 4, DonemTipi.Guz, 3, 5), new CourseSeed("BM443", "Mobil Programlama", 4, DonemTipi.Bahar, 3, 5), new CourseSeed("BM478", "Python ile Veri Bilimine Giriş", 4, DonemTipi.Bahar, 3, 5), new CourseSeed("BM446", "Siber Güvenlik", 4, DonemTipi.Bahar, 3, 5)
        };
       
        courses = courses.GroupBy(course => new { course.Year, course.Term })
            .SelectMany(group => group.Take(5)).ToArray();
      
        var existing = await context.Dersler.Where(d => d.BolumId == department.Id).Select(d => d.DersKodu).ToListAsync();
        context.Dersler.AddRange(courses.Where(c => !existing.Contains(c.Code, StringComparer.OrdinalIgnoreCase)).Select(c => new Ders { DersKodu = c.Code, Adi = c.Name, BolumId = department.Id, Sinif = c.Year, Donem = c.Term, Kredi = c.Credit, Akts = c.Ects }));
        await context.SaveChangesAsync();
    }
}
