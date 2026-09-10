using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using System.Data;

namespace ObsOgrenciBilgiSistemi.Features.StudentCourses.Commands
{
    public class AssignCourseToStudentCommandHandler : IRequestHandler<AssignCourseToStudentCommand, int>
    {
        private readonly AppDbContext _context;

        public AssignCourseToStudentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(AssignCourseToStudentCommand request, CancellationToken cancellationToken)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var mevcutKayit = await _context.OgrenciDersler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.OgrenciId == request.OgrenciId && x.DersId == request.DersId, cancellationToken);

                if (mevcutKayit != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return mevcutKayit.Id;
                }

                var ders = await _context.Dersler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(course => course.Id == request.DersId, cancellationToken)
                    ?? throw new InvalidOperationException("Ders bulunamadı.");

                int currentAkts = await _context.OgrenciDersler
                    .Where(registration => registration.OgrenciId == request.OgrenciId)
                    .SumAsync(registration => registration.Ders.Akts, cancellationToken);

                if (currentAkts + ders.Akts > 40)
                    throw new InvalidOperationException($"En fazla 40 AKTS seçebilirsiniz. Mevcut kayıt: {currentAkts} AKTS.");

                var ogrenciDers = new OgrenciDers
                {
                    OgrenciId = request.OgrenciId,
                    DersId = request.DersId
                };

                _context.OgrenciDersler.Add(ogrenciDers);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ogrenciDers.Id;
            });
        }
    }
}
