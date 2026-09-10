using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Features.Departments.Commands
{
    public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, int>
    {
        private readonly AppDbContext _context;

        public CreateDepartmentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var departmentName = request.Adi?.Trim();
            if (string.IsNullOrWhiteSpace(departmentName))
                throw new InvalidOperationException("Bölüm adı boş bırakılamaz.");

            var normalizedName = departmentName.ToLower();
            var alreadyExists = await _context.Bolumler
                .AnyAsync(department => department.Adi.ToLower() == normalizedName, cancellationToken);
            if (alreadyExists)
                throw new InvalidOperationException("Bu isimde bir bölüm zaten bulunuyor.");

            var bolum = new Bolum { Adi = departmentName };
            _context.Bolumler.Add(bolum);
            await _context.SaveChangesAsync(cancellationToken);
            return bolum.Id;
        }
    }
}
