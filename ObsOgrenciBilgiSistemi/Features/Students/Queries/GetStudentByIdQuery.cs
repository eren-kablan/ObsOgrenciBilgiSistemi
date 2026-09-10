using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetStudentByIdQuery : IRequest<StudentDto>
    {
        public int Id { get; set; }
    }
}