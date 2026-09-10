using MediatR;

namespace ObsOgrenciBilgiSistemi.Features.Devamsizliklar.Queries
{
    public class GetStudentAttendanceQuery : IRequest<List<StudentAttendanceDto>>
    {
        public string StudentId { get; set; }
    }

    public class StudentAttendanceDto
    {
        public string DersAdi { get; set; }
        public int DevamsizlikHaftasi { get; set; }
        public bool Durum { get; set; }
        public DateTime Tarih { get; set; }
    }
}