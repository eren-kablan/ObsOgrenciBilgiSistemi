using MediatR;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetStudentByEmailQuery : IRequest<Ogrenci>
    {
        public string Email { get; set; }
    }
}