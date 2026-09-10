using MediatR;
using ObsOgrenciBilgiSistemi.DTOs;

namespace ObsOgrenciBilgiSistemi.Features.Devamsizliklar.Commands
{
    public class CreateDevamsizlikCommand : IRequest<string>
    {
        public DevamsizlikGirisDto DevamsizlikDto { get; set; } = null!;
    }
}
