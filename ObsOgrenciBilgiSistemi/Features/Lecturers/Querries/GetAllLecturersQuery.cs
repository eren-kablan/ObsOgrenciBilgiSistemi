using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Queries
{
    public class GetAllLecturersQuery : IRequest<List<LecturerDto>>
    {
        public int? BolumId { get; set; }
    }

    public class LecturerDto
    {
        public int Id { get; set; }
        public string Unvani { get; set; } = string.Empty;
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public string Bolumu { get; set; } = string.Empty;
    }
}