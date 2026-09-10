using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Notlar.Queries
{
    public class GetStudentGradesQuery : IRequest<List<StudentGradeDto>>
    {
        public int StudentId { get; set; } 
    }

    public class StudentGradeDto
    {
        public string DersAdi { get; set; } = string.Empty;
        public decimal? Vize { get; set; }
        public decimal? Final { get; set; }
        public decimal? Ortalama { get; set; }
        public string HarfNotu { get; set; } = string.Empty;
    }

    public class GetStudentGradesQueryHandler : IRequestHandler<GetStudentGradesQuery, List<StudentGradeDto>>
    {
        private readonly AppDbContext _context;

        public GetStudentGradesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentGradeDto>> Handle(GetStudentGradesQuery request, CancellationToken cancellationToken)
        {
            var grades = await _context.Notlar
                .Where(n => n.StudentId == request.StudentId)
                .Include(n => n.Ders)
                .Select(n => new StudentGradeDto
                {
                    DersAdi = n.Ders.Adi,
                    Vize = n.Vize,
                    Final = n.Final,
                    Ortalama = n.Ortalama,
                    HarfNotu = n.HarfNotu
                })
                .ToListAsync(cancellationToken);

            return grades;
        }
    }
}
