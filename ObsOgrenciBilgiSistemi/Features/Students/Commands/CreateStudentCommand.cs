using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class CreateStudentCommand : IRequest<int>
    {
        public string Adi { get; set; } = null!;
        public string Soyadi { get; set; } = null!;
        public int BolumId { get; set; }
        public int Sinif { get; set; } 
    }
}
