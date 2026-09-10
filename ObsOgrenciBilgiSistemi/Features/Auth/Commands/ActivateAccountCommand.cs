using MediatR;
using ObsOgrenciBilgiSistemi.DTOs;

namespace ObsOgrenciBilgiSistemi.Features.Auth.Commands
{
    public class ActivateAccountCommand : IRequest<bool>
    {
        public ActivateAccountDto ActivateAccountDto { get; set; } = null!;
    }
}