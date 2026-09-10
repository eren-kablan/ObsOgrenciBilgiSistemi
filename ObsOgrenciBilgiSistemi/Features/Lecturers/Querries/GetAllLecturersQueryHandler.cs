using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Queries
{
    public class GetAllLecturersQueryHandler : IRequestHandler<GetAllLecturersQuery, List<LecturerDto>>
    {
        private readonly AppDbContext _context;

        public GetAllLecturersQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<LecturerDto>> Handle(GetAllLecturersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Akademisyenler.AsQueryable();

           //filtreleme
            if (request.BolumId.HasValue && request.BolumId.Value > 0)
            {
                query = query.Where(a => a.BolumId == request.BolumId.Value);
            }

            return await query
                .Select(lecturers => new LecturerDto
                {
                    Id = lecturers.Id,
                    Unvani = lecturers.Unvani,
                    Adi = lecturers.Adi,
                    Soyadi = lecturers.Soyadi,
                    Email = lecturers.Email,
                    BolumId = lecturers.BolumId,
                    Bolumu = lecturers.Bolum != null ? lecturers.Bolum.Adi : string.Empty
                })
                .ToListAsync(cancellationToken);
        }
    }
}