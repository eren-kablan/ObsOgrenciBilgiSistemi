using MediatR;
using ObsOgrenciBilgiSistemi.Data;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateStudentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
        {
            var student = await _context.Ogrenciler.FindAsync(new object[] { request.Id }, cancellationToken);
            if (student == null)
            {
                return false;
            }

            student.Adi = request.Adi;
            student.Soyadi = request.Soyadi;
            student.OgrenciNumarasi = request.OgrenciNumarasi;
            student.BolumId = request.BolumId;

      
            string ad = request.Adi.Trim().ToLower()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            string soyad = request.Soyadi.Trim().ToLower()
                .Replace("ç", "c").Replace("ğ", "g").Replace("ı", "i")
                .Replace("ö", "o").Replace("ş", "s").Replace("ü", "u");

            student.Email = $"{ad}.{soyad}@ogr.duzce.edu.tr";

            _context.Ogrenciler.Update(student);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}