using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetStudentByEmailQueryHandler : IRequestHandler<GetStudentByEmailQuery, Ogrenci?>
    {
        private readonly AppDbContext _context;

        public GetStudentByEmailQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Ogrenci?> Handle(GetStudentByEmailQuery request, CancellationToken cancellationToken)
        {
            return await _context.Ogrenciler
                .Include(o => o.Bolum)
                .FirstOrDefaultAsync(o => o.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        }
    }
}
