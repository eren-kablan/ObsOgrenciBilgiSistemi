using MediatR;
using ObsOgrenciBilgiSistemi.DTOs;

namespace ObsOgrenciBilgiSistemi.Features.Courses.Queries
{
    public class GetAllCoursesQuery : IRequest<List<CourseDto>>
    {
    }
}