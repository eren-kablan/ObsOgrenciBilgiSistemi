using MediatR;
using System.Collections.Generic;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetAllStudentsQuery : IRequest<List<StudentDto>>
    {
        public int? DersId { get; set; } 
        public int? BolumId { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string Adi { get; set; }
        public string Soyadi { get; set; }
        public string Email { get; set; }
        public string OgrenciNumarasi { get; set; }
        public int BolumId { get; set; }
        public string BolumAdi { get; set; } = string.Empty;
        public int Sinif { get; set; }
        public int? DersId { get; set; }
    }
}
