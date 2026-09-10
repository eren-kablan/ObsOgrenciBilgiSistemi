using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
namespace ObsOgrenciBilgiSistemi.Features.Courses.Queries
{
    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, List<CourseDto>>
    {
        private readonly AppDbContext _context;
            
        public GetAllCoursesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Dersler
                .Include(d => d.Bolum)
                .Select(d => new CourseDto
                {
                    Id = d.Id,
                    DersKodu = d.DersKodu,
                    Adi = d.Adi,
                    Kredi = d.Kredi,
                    Akts = d.Akts,
                    BolumId = d.BolumId,      
                    BolumAdi = d.Bolum.Adi,
                    AkademisyenEmail = d.AkademisyenEmail,
                    AkademisyenAdi = d.Akademisyen == null
                        ? null
                        : d.Akademisyen.Unvani + " " + d.Akademisyen.Adi + " " + d.Akademisyen.Soyadi,
                    Sinif = d.Sinif,      
                    Donem = d.Donem 
                })
                .ToListAsync(cancellationToken);
        }
    }
}
