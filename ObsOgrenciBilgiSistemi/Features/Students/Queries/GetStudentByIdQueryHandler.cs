using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto?>
    {
        private readonly AppDbContext _context;

        public GetStudentByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudentDto?> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.Ogrenciler
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity == null)
                return null;

            return new StudentDto
            {
                Id = entity.Id,
                Adi = entity.Adi,
                Soyadi = entity.Soyadi,
                OgrenciNumarasi = entity.OgrenciNumarasi,
                BolumId = entity.BolumId,
                Sinif = entity.Sinif,
                Email = entity.Email ?? string.Empty
            };
        }
    }
}
