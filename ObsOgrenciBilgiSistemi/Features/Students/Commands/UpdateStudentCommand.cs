using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class UpdateStudentCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public string OgrenciNumarasi { get; set; }
        public int BolumId { get; set; }
    }
}