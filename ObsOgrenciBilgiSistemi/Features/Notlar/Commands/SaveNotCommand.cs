using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Features.Notlar.Commands
{
    public class SaveNotCommand : IRequest<bool>
    {
        public NotGirisDto NotDto { get; set; } = null!;
    }

    public class SaveNotCommandHandler : IRequestHandler<SaveNotCommand, bool>
    {
        private readonly AppDbContext _context;

        public SaveNotCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(SaveNotCommand request, CancellationToken cancellationToken)
        {
            var dto = request.NotDto;

            var student = await _context.Ogrenciler
                .FirstOrDefaultAsync(u => u.OgrenciNumarasi == dto.OgrenciNo, cancellationToken);

            if (student == null) return false;

            var ders = await _context.Dersler
                .FirstOrDefaultAsync(d => d.DersKodu == dto.DersKodu, cancellationToken);

            if (ders == null) return false;

            

            if (!await _context.OgrenciDersler.AnyAsync(od => od.OgrenciId == student.Id && od.DersId == ders.Id, cancellationToken)) return false;
            var notKaydi = await _context.Notlar
                .FirstOrDefaultAsync(n => n.StudentId == student.Id && n.DersId == ders.Id, cancellationToken);

            if (notKaydi == null)
            {
                notKaydi = new Not
                {
                    StudentId = student.Id, 
                    DersId = ders.Id
                };
                _context.Notlar.Add(notKaydi);
            }

            if (dto.Vize.HasValue) notKaydi.Vize = dto.Vize.Value;
            if (dto.Final.HasValue) notKaydi.Final = dto.Final.Value;

            decimal vize = notKaydi.Vize ?? 0m;
            decimal final = notKaydi.Final ?? 0m;
            decimal ortalama = (vize * 0.4m) + (final * 0.6m);

            notKaydi.Ortalama = ortalama;
            notKaydi.HarfNotu = CalculateHarfNotu(ortalama);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private string CalculateHarfNotu(decimal ort) => ort switch
        {
            >= 90 => "AA",
            >= 85 => "BA",
            >= 80 => "BB",
            >= 75 => "CB",
            >= 70 => "CC",
            >= 65 => "DC",
            >= 60 => "DD",
            >= 50 => "FD",
            _ => "FF"
        };
    }
}
