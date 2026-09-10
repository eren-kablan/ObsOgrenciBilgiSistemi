using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class CreateLecturerCommand : IRequest<CreateLecturerResponseDto>
    {
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string Unvani { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BolumId { get; set; }
    }

    public class CreateLecturerResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;
    }
}