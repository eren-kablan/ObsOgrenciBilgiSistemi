using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
    {
        private readonly AppDbContext _context;

        public DeleteStudentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var student = await _context.Ogrenciler
                    .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
                if (student == null)
                {
                    return false;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                // Öğrenciye bağlı kayıtlar
                await _context.DersKayitTalepDersleri
                    .Where(item => item.DersKayitTalebi.OgrenciId == student.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.DersKayitTalepleri
                    .Where(item => item.OgrenciId == student.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Notlar
                    .Where(item => item.StudentId == student.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Devamsizliklar
                    .Where(item => item.StudentId == student.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.OgrenciDersler
                    .Where(item => item.OgrenciId == student.Id)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Bildirimler
                    .Where(item => item.AliciEmail == student.Email)
                    .ExecuteDeleteAsync(cancellationToken);
                await _context.Users
                    .Where(item => item.Email == student.Email)
                    .ExecuteDeleteAsync(cancellationToken);

                // Öğrenci hesabı
                _context.Ogrenciler.Remove(student);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }
    }
}
