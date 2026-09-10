using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class DeleteStudentCommand : IRequest<bool>
    {
        public int Id { get; set; } 
    }
}