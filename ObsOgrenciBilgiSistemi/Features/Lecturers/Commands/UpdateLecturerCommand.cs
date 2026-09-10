using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class UpdateLecturerCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Unvani { get; set; } = string.Empty;
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BolumId { get; set; }
    }
}