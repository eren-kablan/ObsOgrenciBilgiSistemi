using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Departments.Queries
{
    public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
    {
        private readonly AppDbContext _context;

        public GetAllDepartmentsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Bolumler
                .OrderBy(b => b.Id)
                .Select(b => new DepartmentDto
                {
                    Id = b.Id,
                    Adi = b.Adi
                })
                .ToListAsync(cancellationToken);
           
        }
    }
}
