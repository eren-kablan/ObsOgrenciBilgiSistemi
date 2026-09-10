using MediatR;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;
using ObsOgrenciBilgiSistemi.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public CreateStudentCommandHandler(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            // Kurumsal e-posta ve aktivasyon tokenı
            string cleanName = request.Adi.ToLower().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c").Replace(" ", "");
            string cleanSurname = request.Soyadi.ToLower().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c").Replace(" ", "");

            string email = $"{cleanName}.{cleanSurname}@ogr.duzce.edu.tr";
            string activationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            Ogrenci? yeniOgrenci = null;
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                // Öğrenci numarası
                string prefix = $"{DateTime.UtcNow:yy}{request.BolumId:D2}";
                var existingNumbers = await _context.Ogrenciler
                    .AsNoTracking()
                    .Where(student => student.OgrenciNumarasi.StartsWith(prefix))
                    .Select(student => student.OgrenciNumarasi)
                    .ToListAsync(cancellationToken);

                int nextSequence = existingNumbers
                    .Select(number => int.TryParse(number[prefix.Length..], out int sequence) ? sequence : 0)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                yeniOgrenci = new Ogrenci
                {
                    Adi = request.Adi,
                    Soyadi = request.Soyadi,
                    OgrenciNumarasi = $"{prefix}{nextSequence:D5}",
                    Email = email,
                    BolumId = request.BolumId,
                    Sinif = request.Sinif,
                    ActivationTokenHash = HashToken(activationToken),
                    ActivationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
                    ActivationEmailSentAt = DateTime.UtcNow,
                    IsActive = false
                };

                _context.Ogrenciler.Add(yeniOgrenci);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            });

            if (yeniOgrenci is null)
                throw new InvalidOperationException("Öğrenci kaydı oluşturulamadı.");

            // Aktivasyon e-postası
            await _emailService.SendActivationEmailAsync(
                yeniOgrenci.Email,
                $"{yeniOgrenci.Adi} {yeniOgrenci.Soyadi}",
                activationToken
            );

            return yeniOgrenci.Id;
        }

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
    }
