using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class CreateStudentCommand : IRequest<int>
    {
        [Required(ErrorMessage = "Öğrenci adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Öğrenci adı 2-100 karakter arasında olmalıdır.")]
        public string Adi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci soyadı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Öğrenci soyadı 2-100 karakter arasında olmalıdır.")]
        public string Soyadi { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir bölüm seçilmelidir.")]
        public int BolumId { get; set; }

        [Range(1, 4, ErrorMessage = "Sınıf 1 ile 4 arasında olmalıdır.")]
        public int Sinif { get; set; } 
    }
}
