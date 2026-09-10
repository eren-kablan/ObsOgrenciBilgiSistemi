using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class UpdateLecturerCommandHandler : IRequestHandler<UpdateLecturerCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateLecturerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateLecturerCommand request, CancellationToken cancellationToken)
        {
            var lecturer = await _context.Akademisyenler
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (lecturer == null) return false;

            lecturer.Unvani = request.Unvani;
            lecturer.Adi = request.Adi;
            lecturer.Soyadi = request.Soyadi;
            lecturer.Email = request.Email;
            lecturer.BolumId = request.BolumId;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}