using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class DeleteLecturerCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}