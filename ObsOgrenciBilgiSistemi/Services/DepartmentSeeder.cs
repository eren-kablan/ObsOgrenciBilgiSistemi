using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Services;

public static class DepartmentSeeder
{
    // Düzce Üniversitesi EBS lisans programları ve projede korunan mevcut bölüm.
    private static readonly string[] DepartmentNames =
    [
        "Bilgisayar Mühendisliği",
        "Elektrik Elektronik Mühendisliği",
        "Endüstri Mühendisliği",
        "Hemşirelik",
        "Tıp",
        "Mekatronik Mühendisliği",
        "Yapay Zeka Mühendisliği",
        "Matematik",
        "Abaza Dili ve Edebiyatı",
        "Ağaç İşleri Endüstri Mühendisliği",
        "Antrenörlük Eğitimi",
        "Arkeoloji",
        "Beden Eğitimi ve Spor Öğr.",
        "Beden Eğitimi ve Spor Öğretmenliği",
        "Beslenme ve Diyetetik",
        "Bilgisayar Mühendisliği (İngilizce)",
        "Bitki Koruma",
        "Biyoloji",
        "Biyomedikal Mühendisliği",
        "Biyosistem Mühendisliği",
        "Çerkez Dili ve Edebiyatı",
        "Çevre Mühendisliği",
        "Eczacılık",
        "Eczacılık Meslek Bilimleri",
        "Eczacılık Teknolojisi",
        "Felsefe",
        "Fen Bilgisi Öğretmenliği",
        "Fizik",
        "Gastronomi ve Mutfak Sanatları",
        "Görsel İletişim Tasarımı",
        "Gürcü Dili ve Edebiyatı",
        "Heykel",
        "İktisat",
        "İlahiyat",
        "İlköğretim Matematik Öğretmenliği",
        "İmalat Mühendisliği",
        "İngilizce Öğretmenliği",
        "İnşaat Mühendisliği",
        "İşletme",
        "Kimya",
        "Makine Mühendisliği",
        "Makine ve İmalat Mühendisliği",
        "Mimarlık",
        "Modern Diller ve Temel Yabancı Diller",
        "Mütercim - Tercümanlık",
        "Okul Öncesi Öğretmenliği",
        "Orman Endüstrisi Mühendisliği",
        "Orman Mühendisliği",
        "Özel Eğitim Öğretmenliği",
        "Peyzaj Mimarlığı",
        "Psikoloji",
        "Radyo, Televizyon ve Sinema",
        "Rehberlik ve Psikolojik Danışmanlık",
        "Resim",
        "Sağlık Yönetimi",
        "Sınıf Öğretmenliği",
        "Sigortacılık ve Sosyal Güvenlik",
        "Siyaset Bilimi ve Kamu Yönetimi",
        "Siyaset Bilimi ve Kamu Yönetimi (İngilizce)",
        "Sosyal Hizmet",
        "Sosyoloji",
        "Spor Yöneticiliği",
        "Tarımsal Biyoteknoloji",
        "Tarih",
        "Tarla Bitkileri",
        "Temel Eczacılık Bilimleri",
        "Tiyatro Eleştirmenliği ve Dramaturji",
        "Turizm İşletmeciliği",
        "Turizm İşletmeciliği ve Otelcilik",
        "Türk Dili ve Edebiyatı",
        "Türk Müziği",
        "Türkçe Eğitimi",
        "Uluslararası İlişkiler",
        "Uluslararası Ticaret",
        "Uluslararası Ticaret ve Finansman",
        "Yönetim Bilişim Sistemleri"
    ];

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var existingNames = await context.Bolumler
            .AsNoTracking()
            .Select(department => department.Adi)
            .ToListAsync(cancellationToken);
        var normalizedExistingNames = existingNames
            .Select(NormalizeName)
            .ToHashSet(StringComparer.Ordinal);

        var missingDepartments = DepartmentNames
            .Where(name => normalizedExistingNames.Add(NormalizeName(name)))
            .Select(name => new Bolum { Adi = name })
            .ToList();

        if (missingDepartments.Count == 0) return;

        context.Bolumler.AddRange(missingDepartments);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeName(string value)
    {
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character == 'ı' ? 'i' : character));
        }

        return builder.ToString();
    }
}
