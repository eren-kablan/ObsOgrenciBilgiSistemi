using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Enums;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Services;

public sealed class DuzceCurriculumImportService
{
    private const string BaseUrl = "https://ebs.duzce.edu.tr";
    private const string UndergraduateProgramsPath = "/tr-TR/Program/Index/2";
    private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(500);

    private readonly AppDbContext _context;
    private readonly ILogger<DuzceCurriculumImportService> _logger;

    public DuzceCurriculumImportService(
        AppDbContext context,
        ILogger<DuzceCurriculumImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CurriculumImportResult> ImportAllAsync(CancellationToken cancellationToken)
    {
        var result = new CurriculumImportResult();
        var sourcePrograms = (await GetProgramsAsync(cancellationToken))
            .Where(program => program.IsNormalEducation)
            .GroupBy(program => NormalizeName(program.Name), StringComparer.Ordinal)
            .Select(group => group
                .OrderBy(program => program.Name.Length)
                .ThenBy(program => program.Name, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderBy(program => GetDepartmentName(program.Name), StringComparer.Create(new CultureInfo("tr-TR"), true))
            .ToList();

        if (sourcePrograms.Count == 0)
            throw new InvalidOperationException("EBS lisans programları listesinde normal öğretim programı bulunamadı.");
        result.SourceProgramCount = sourcePrograms.Count;

        var departments = await _context.Bolumler
            .OrderBy(department => department.Adi)
            .ToListAsync(cancellationToken);
        var departmentsByName = departments
            .GroupBy(department => NormalizeName(department.Adi), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var importTargets = new List<(Bolum Department, ProgramLink Program)>();

        foreach (var program in sourcePrograms)
        {
            var normalizedName = NormalizeName(program.Name);
            if (!departmentsByName.TryGetValue(normalizedName, out var department))
            {
                department = new Bolum { Adi = GetDepartmentName(program.Name) };
                _context.Bolumler.Add(department);
                departments.Add(department);
                departmentsByName.Add(normalizedName, department);
                result.AddedDepartmentCount++;
            }
            else
            {
                result.MatchedDepartmentCount++;
            }

            importTargets.Add((department, program));
        }

        var sourceProgramNames = sourcePrograms
            .Select(program => NormalizeName(program.Name))
            .ToHashSet(StringComparer.Ordinal);
        result.UnmatchedDepartments.AddRange(departments
            .Where(department => !sourceProgramNames.Contains(NormalizeName(department.Adi)))
            .Select(department => department.Adi));

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var (department, program) in importTargets)
        {
            try
            {
                var curriculum = await GetLatestCurriculumAsync(program, cancellationToken);
                if (curriculum is null || curriculum.Courses.Count == 0)
                {
                    result.Errors.Add($"{department.Adi}: Yayımlanmış aktif müfredat bulunamadı.");
                    continue;
                }

                var existingCourseList = await _context.Dersler
                    .Where(course => course.BolumId == department.Id)
                    .ToListAsync(cancellationToken);
                var existingCourses = existingCourseList.ToLookup(
                    course => course.DersKodu,
                    StringComparer.OrdinalIgnoreCase);

                foreach (var imported in curriculum.Courses)
                {
                    var matches = existingCourses[imported.Code].ToList();
                    if (matches.Count > 0)
                    {
                        foreach (var existing in matches)
                        {
                            existing.Adi = imported.Name;
                            existing.Sinif = imported.Year;
                            existing.Donem = imported.Term;
                            existing.Kredi = imported.Credit;
                            existing.Akts = imported.Ects;
                            result.UpdatedCourseCount++;
                        }
                        continue;
                    }

                    _context.Dersler.Add(new Ders
                    {
                        DersKodu = imported.Code,
                        Adi = imported.Name,
                        BolumId = department.Id,
                        Sinif = imported.Year,
                        Donem = imported.Term,
                        Kredi = imported.Credit,
                        Akts = imported.Ects
                    });
                    result.AddedCourseCount++;
                }

                await _context.SaveChangesAsync(cancellationToken);
                result.ImportedDepartments.Add(new ImportedDepartment(
                    department.Adi, program.Name, curriculum.AcademicYear, curriculum.Courses.Count));
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
            {
                _logger.LogWarning(exception, "{Department} müfredatı içe aktarılamadı.", department.Adi);
                result.Errors.Add($"{department.Adi}: {exception.Message}");
            }

            await Task.Delay(RequestDelay, cancellationToken);
        }

        return result;
    }

    private static string NormalizeName(string value)
    {
        var decoded = WebUtility.HtmlDecode(value);
        decoded = Regex.Replace(decoded, @"\([^)]*(öğretim|education)[^)]*\)", string.Empty, RegexOptions.IgnoreCase);
        var decomposed = decoded.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
                builder.Append(char.ToLowerInvariant(character == 'ı' ? 'i' : character));
        }
        return builder.ToString();
    }

    private static string GetDepartmentName(string programName) =>
        Regex.Replace(
            CleanText(programName),
            @"\s*\(\s*Normal\s+Öğretim\s*\)\s*$",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Trim();

    private async Task<IReadOnlyList<ProgramLink>> GetProgramsAsync(CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        var html = await GetHtmlWithRetryAsync(
            client,
            UndergraduateProgramsPath,
            "/Bolum/OgretimProgrami/",
            cancellationToken);
        var document = LoadHtml(html);
        var links = document.DocumentNode.SelectNodes(
                "//a[contains(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '/bolum/ogretimprogrami/')]")
            ?? throw new InvalidOperationException("EBS lisans programları listesi okunamadı.");

        return links
            .Select(link => new ProgramLink(
                CleanText(link.InnerText),
                WebUtility.HtmlDecode(link.GetAttributeValue("href", string.Empty))))
            .Where(program => !string.IsNullOrWhiteSpace(program.Name) && !string.IsNullOrWhiteSpace(program.Path))
            .DistinctBy(program => program.Path)
            .ToList();
    }

    private async Task<CurriculumPage?> GetLatestCurriculumAsync(
        ProgramLink program,
        CancellationToken cancellationToken)
    {
        using var client = CreateClient();
        var initialHtml = await GetHtmlWithRetryAsync(client, program.Path, "BolognaYil", cancellationToken);
        var initialDocument = LoadHtml(initialHtml);
        var years = initialDocument.DocumentNode
            .SelectNodes("//select[@id='BolognaYil']/option")?
            .Select(option => new
            {
                Number = option.GetAttributeValue("value", string.Empty),
                Name = CleanText(option.InnerText)
            })
            .Where(year => int.TryParse(year.Number, out _))
            .OrderByDescending(year => int.Parse(year.Number, CultureInfo.InvariantCulture))
            .ToList() ?? [];

        var bot = GetQueryValue(program.Path, "bot")
            ?? throw new InvalidOperationException("Program öğretim türü bulunamadı.");

        foreach (var year in years)
        {
            using var response = await client.PostAsJsonAsync("/tr-TR/Home/BolognaYilGuncelle", new
            {
                yilNo = year.Number,
                bolumOgretimTurNo = bot,
                returnURL = program.Path
            }, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await GetHtmlWithRetryAsync(client, program.Path, "BolognaYil", cancellationToken);
            var document = LoadHtml(html);
            var curriculumOption = document.DocumentNode.SelectSingleNode("//select[@id='Mufredat']/option[@value!='0']");
            if (curriculumOption is null) continue;

            var courses = ParseCourses(document);
            if (courses.Count > 0) return new CurriculumPage(year.Name, courses);
        }

        return null;
    }

    private static List<ImportedCourse> ParseCourses(HtmlDocument document)
    {
        var tables = document.DocumentNode.SelectNodes(
            "//table[thead/tr/th[normalize-space(.)='Kodu'] and thead/tr/th[contains(normalize-space(.), 'AKTS')]]")?.ToList() ?? [];
        var courses = new List<ImportedCourse>();

        for (var tableIndex = 0; tableIndex < tables.Count; tableIndex++)
        {
            var semester = tableIndex + 1;
            var year = Math.Min(4, ((semester - 1) / 2) + 1);
            var term = semester % 2 == 1 ? DonemTipi.Guz : DonemTipi.Bahar;
            var rows = tables[tableIndex].SelectNodes(".//tbody/tr[starts-with(@id, 'dersRow_')]")?.ToList() ?? [];

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./td");
                if (cells is null || cells.Count < 7) continue;

                var code = CleanText(cells[0].InnerText);
                var name = CleanText(cells[1].InnerText);
                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)) continue;
                if (!TryParseNumber(cells[5].InnerText, out var credit) || !TryParseNumber(cells[6].InnerText, out var ects)) continue;

                courses.Add(new ImportedCourse(code, name, year, term, credit, ects));
            }
        }

        return courses.DistinctBy(course => course.Code, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool TryParseNumber(string value, out int result)
    {
        var normalized = CleanText(value).Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            result = (int)Math.Round(number, MidpointRounding.AwayFromZero);
            return true;
        }
        result = 0;
        return false;
    }

    private static string? GetQueryValue(string path, string name)
    {
        var queryStart = path.IndexOf('?');
        if (queryStart < 0) return null;
        return path[(queryStart + 1)..].Split('&')
            .Select(part => part.Split('=', 2))
            .Where(part => part.Length == 2)
            .FirstOrDefault(part => part[0].Equals(name, StringComparison.OrdinalIgnoreCase))?[1];
    }

    private static HtmlDocument LoadHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }

    private static async Task<string> GetHtmlWithRetryAsync(
        HttpClient client,
        string path,
        string expectedContent,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(path, HttpCompletionOption.ResponseContentRead, cancellationToken);
                response.EnsureSuccessStatusCode();
                var html = await response.Content.ReadAsStringAsync(cancellationToken);
                if (html.Contains(expectedContent, StringComparison.OrdinalIgnoreCase)) return html;

                lastException = new InvalidOperationException(
                    $"EBS beklenen sayfa yerine farklı bir içerik döndürdü (deneme {attempt}/3).");
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                lastException = exception;
            }

            if (attempt < 3)
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancellationToken);
        }

        throw new InvalidOperationException(
            "EBS sayfası şu anda okunamıyor. Lütfen kısa bir süre sonra yeniden deneyin.",
            lastException);
    }

    private static string CleanText(string value) =>
        Regex.Replace(WebUtility.HtmlDecode(value), @"\s+", " ").Trim();

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.All
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Duzce-OBS-Curriculum-Importer/1.0 (educational project; low-frequency admin import)");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr-TR,tr;q=0.9");
        return client;
    }

    private sealed record ProgramLink(string Name, string Path)
    {
        public bool IsNormalEducation => Name.Contains("Normal Öğretim", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record CurriculumPage(string AcademicYear, List<ImportedCourse> Courses);
    private sealed record ImportedCourse(string Code, string Name, int Year, DonemTipi Term, int Credit, int Ects);
}

public sealed class CurriculumImportResult
{
    public int SourceProgramCount { get; set; }
    public int AddedDepartmentCount { get; set; }
    public int MatchedDepartmentCount { get; set; }
    public int AddedCourseCount { get; set; }
    public int UpdatedCourseCount { get; set; }
    public List<ImportedDepartment> ImportedDepartments { get; } = [];
    public List<string> UnmatchedDepartments { get; } = [];
    public List<string> Errors { get; } = [];
}

public sealed record ImportedDepartment(
    string DepartmentName,
    string SourceProgramName,
    string AcademicYear,
    int CourseCount);
