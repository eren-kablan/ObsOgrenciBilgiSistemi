using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class DeleteLecturerCommandHandler : IRequestHandler<DeleteLecturerCommand, bool>
    {
        private readonly AppDbContext _context;

        public DeleteLecturerCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteLecturerCommand request, CancellationToken cancellationToken)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var lecturer = await _context.Akademisyenler
                    .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

                if (lecturer == null) return false;

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                // danismanlik ve ders atamalari.
                await _context.Bolumler
                    .Where(department => department.DanismanAkademisyenId == lecturer.Id)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(department => department.DanismanAkademisyenId, (int?)null),
                        cancellationToken);
                await _context.Dersler
                    .Where(course => course.AkademisyenId == lecturer.Id)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(course => course.AkademisyenId, (int?)null)
                        .SetProperty(course => course.AkademisyenEmail, (string?)null),
                        cancellationToken);
                // akademisyene bagli talepler
                await _context.DersKayitTalepDersleri
                    .Where(item => item.DersKayitTalebi.DanismanAkademisyenId == lecturer.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.DersKayitTalepleri
                    .Where(request => request.DanismanAkademisyenId == lecturer.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.DersTalepleri
                    .Where(courseRequest => courseRequest.AkademisyenId == lecturer.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Bildirimler
                    .Where(notification => notification.AliciEmail == lecturer.Email)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Users
                    .Where(user => user.Email == lecturer.Email)
                    .ExecuteDeleteAsync(cancellationToken);
                // akademisyen hesabi
                _context.Akademisyenler.Remove(lecturer);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });

        }
    }
}
