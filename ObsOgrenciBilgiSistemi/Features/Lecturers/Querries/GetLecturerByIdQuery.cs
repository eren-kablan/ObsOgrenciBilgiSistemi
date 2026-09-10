using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Queries
{
    public class GetLecturerByIdQuery : IRequest<LecturerDto?>
    {
        public int Id { get; set; }
    }
}
