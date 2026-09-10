using MediatR;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Features.Devamsizliklar.Commands
{
    public class CreateDevamsizlikCommandHandler : IRequestHandler<CreateDevamsizlikCommand, string>
    {
        private readonly AppDbContext _context;

        public CreateDevamsizlikCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(CreateDevamsizlikCommand request, CancellationToken cancellationToken)
        {
            var dto = request.DevamsizlikDto;

            var devamsizlik = new Devamsizlik
            {
                StudentId = int.Parse(dto.StudentId),
                DersId = dto.DersId,
                DevamsizlikHaftasi = dto.DevamsizlikHaftasi,
                Durum = dto.Durum
            };

            _context.Devamsizliklar.Add(devamsizlik);
            await _context.SaveChangesAsync(cancellationToken);

            return "Devamsızlık kaydı başarıyla eklendi.";
        }
    }
}