using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Courses.Commands
{
    public class AssignLecturerToCourseCommand : IRequest<bool>
    {
        public int DersId { get; set; }
        public string AkademisyenEmail { get; set; } = string.Empty;
    }

    public class AssignLecturerToCourseCommandHandler : IRequestHandler<AssignLecturerToCourseCommand, bool>
    {
        private readonly AppDbContext _context;

        public AssignLecturerToCourseCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(AssignLecturerToCourseCommand request, CancellationToken cancellationToken)
        {
            var ders = await _context.Dersler.FirstOrDefaultAsync(d => d.Id == request.DersId, cancellationToken);
            if (ders == null) return false;

            ders.AkademisyenEmail = request.AkademisyenEmail;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}