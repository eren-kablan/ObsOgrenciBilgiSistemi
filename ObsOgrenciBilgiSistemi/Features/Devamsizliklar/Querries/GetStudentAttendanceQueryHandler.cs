using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Devamsizliklar.Queries
{
    public class GetStudentAttendanceQueryHandler : IRequestHandler<GetStudentAttendanceQuery, List<StudentAttendanceDto>>
    {
        private readonly AppDbContext _context;

        public GetStudentAttendanceQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentAttendanceDto>> Handle(GetStudentAttendanceQuery request, CancellationToken cancellationToken)
        {
            // gelen stringi inte cevirdik.
            int parsedStudentId = int.Parse(request.StudentId);
          
            var list = await _context.Devamsizliklar
                .Where(x => x.StudentId == parsedStudentId) 
                .Include(x => x.Ders)
                .Select(x => new StudentAttendanceDto
                {
                    DersAdi = x.Ders.Adi,
                    DevamsizlikHaftasi = x.DevamsizlikHaftasi,
                    Durum = x.Durum,
                    Tarih = x.Tarih
                })
                .ToListAsync(cancellationToken);

            return list;
        }
    }
}