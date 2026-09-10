using MediatR;
using ObsOgrenciBilgiSistemi.Enums;

namespace ObsOgrenciBilgiSistemi.Features.Courses.Commands
{
    public class CreateCourseCommand : IRequest<int>
    {
        public string Adi { get; set; } = string.Empty;
        public int BolumId { get; set; }
        public int Sinif { get; set; } = 1;
        public int Akts { get; set; } = 5;
        public DonemTipi Donem { get; set; } = DonemTipi.Guz; //enumu cagirdik
    }
}
