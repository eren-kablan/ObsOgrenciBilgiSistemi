using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Departments.Queries
{
    public class GetAllDepartmentsQuery : IRequest<List<DepartmentDto>>
    {
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
    }
}
