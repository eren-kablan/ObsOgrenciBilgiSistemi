using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class UpdateStudentCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Adi { get; set; } = string.Empty;
        public string Soyadi { get; set; } = string.Empty;
        public string OgrenciNumarasi { get; set; } = string.Empty;
        public int BolumId { get; set; }
    }
}
