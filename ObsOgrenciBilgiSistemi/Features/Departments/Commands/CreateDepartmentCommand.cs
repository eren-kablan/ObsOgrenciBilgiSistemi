using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Departments.Commands
{
    public class CreateDepartmentCommand : IRequest<int>
    {
        public string Adi { get; set; } = string.Empty;
    }
}
