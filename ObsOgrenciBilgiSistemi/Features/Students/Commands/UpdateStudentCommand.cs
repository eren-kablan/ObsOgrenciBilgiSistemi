using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ObsOgrenciBilgiSistemi.Features.Students.Commands
{
    public class UpdateStudentCommand : IRequest<bool>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir öğrenci seçilmelidir.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Öğrenci adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Öğrenci adı 2-100 karakter arasında olmalıdır.")]
        public string Adi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci soyadı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Öğrenci soyadı 2-100 karakter arasında olmalıdır.")]
        public string Soyadi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "Öğrenci numarası 4-20 karakter arasında olmalıdır.")]
        public string OgrenciNumarasi { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir bölüm seçilmelidir.")]
        public int BolumId { get; set; }
    }
}
