using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Features.Courses.Commands
{
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, int>
    {
        private readonly AppDbContext _context;

        public CreateCourseCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            // 1. Bölüm kısaltması
            string prefix = request.BolumId switch
            {
                1 => "BM",
                2 => "EM",
                3 => "EEM",
                4 => "MM",
                _ => "DERS"
            };

            // 2. Sınıfa göre başlangıç tabanı (1. sınıf -> 100, 2. sınıf -> 200, 4. sınıf -> 400)
            int sinifTabani = request.Sinif * 100;

          
            var sonDersBuSinifta = await _context.Dersler
                .Where(d => d.BolumId == request.BolumId && d.DersKodu.StartsWith(prefix))
                .ToListAsync(cancellationToken);

            // İlgili sınıf aralığındaki dersleri filtrele 
            var sinifDersleri = sonDersBuSinifta.Where(d => {
                var numericPart = new string(d.DersKodu.Where(char.IsDigit).ToArray());
                if (int.TryParse(numericPart, out int sayi))
                {
                    return sayi >= sinifTabani && sayi < sinifTabani + 100;
                }
                return false;
            }).OrderByDescending(d => d.Id).FirstOrDefault();

            string yeniKod = "";
            if (sinifDersleri == null)
            {
                // Bu sınıfta ilk defa ders açılıyorsa tabandan başla 
                yeniKod = $"{prefix}{sinifTabani}";
            }
            else
            {
                // Bu sınıfta zaten ders varsa sonrakini al 
                var numericPart = new string(sinifDersleri.DersKodu.Where(char.IsDigit).ToArray());
                if (int.TryParse(numericPart, out int sonSayi))
                {
                    yeniKod = $"{prefix}{sonSayi + 1}";
                }
                else
                {
                    yeniKod = $"{prefix}{sinifTabani}";
                }
            }

            // 4. Yeni dersi kaydet 
            var ders = new Ders
            {
                Adi = request.Adi,
                DersKodu = yeniKod,
                BolumId = request.BolumId,
                Sinif = request.Sinif,
                Akts = request.Akts,
                Donem = request.Donem 
                
            };

            _context.Dersler.Add(ders);
            await _context.SaveChangesAsync(cancellationToken);

            return ders.Id;
        }
    }
}
