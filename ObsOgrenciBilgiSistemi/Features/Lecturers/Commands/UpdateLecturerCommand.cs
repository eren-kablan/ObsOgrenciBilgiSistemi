using MediatR;
using System.ComponentModel.DataAnnotations;

namespace ObsOgrenciBilgiSistemi.Features.Lecturers.Commands
{
    public class UpdateLecturerCommand : IRequest<bool>
    {
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir akademisyen seçilmelidir.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Akademik unvan zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Akademik unvan 2-50 karakter arasında olmalıdır.")]
        public string Unvani { get; set; } = string.Empty;

        [Required(ErrorMessage = "Akademisyen adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Akademisyen adı 2-100 karakter arasında olmalıdır.")]
        public string Adi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Akademisyen soyadı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Akademisyen soyadı 2-100 karakter arasında olmalıdır.")]
        public string Soyadi { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girilmelidir.")]
        public string Email { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir bölüm seçilmelidir.")]
        public int BolumId { get; set; }
    }
}
