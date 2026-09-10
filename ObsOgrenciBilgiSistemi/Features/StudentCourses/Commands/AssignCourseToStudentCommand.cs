using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.StudentCourses.Commands
{
    public class AssignCourseToStudentCommand : IRequest<int>
    {
        public int OgrenciId { get; set; }
        public int DersId { get; set; }
    }
}