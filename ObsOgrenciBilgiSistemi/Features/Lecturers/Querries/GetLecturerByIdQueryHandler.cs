using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Queries
{
    public class GetLecturerByIdQueryHandler : IRequestHandler<GetLecturerByIdQuery, LecturerDto?>
    {
        private readonly AppDbContext _context;

        public GetLecturerByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LecturerDto?> Handle(GetLecturerByIdQuery request, CancellationToken cancellationToken)
        {
            var lecturer = await _context.Akademisyenler
                .Include(a => a.Bolum)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (lecturer == null) return null;

            return new LecturerDto
            {
                Id = lecturer.Id,
                Unvani = lecturer.Unvani,
                Adi = lecturer.Adi,
                Soyadi = lecturer.Soyadi,
                Email = lecturer.Email,
                BolumId = lecturer.BolumId,
                Bolumu = lecturer.Bolum != null ? lecturer.Bolum.Adi : string.Empty
            };
        }
    }
}
