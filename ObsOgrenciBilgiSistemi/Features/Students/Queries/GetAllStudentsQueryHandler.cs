using MediatR;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Data;
using ObsOgrenciBilgiSistemi.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ObsOgrenciBilgiSistemi.Features.Students.Queries
{
    public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, List<StudentDto>>
    {
        private readonly AppDbContext _context;

        public GetAllStudentsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
        {
            
            if (request.DersId.HasValue && request.DersId.Value > 0)
            {
                var courseStudents = _context.OgrenciDersler
                    .Where(od => od.DersId == request.DersId.Value);
                if (request.BolumId.HasValue && request.BolumId.Value > 0)
                {
                    courseStudents = courseStudents.Where(od => od.Ogrenci.BolumId == request.BolumId.Value);
                }

                return await courseStudents
                    .Select(od => new StudentDto
                    {
                        Id = od.Ogrenci.Id,
                        Adi = od.Ogrenci.Adi,
                        Soyadi = od.Ogrenci.Soyadi,
                        Email = od.Ogrenci.Email,
                        OgrenciNumarasi = od.Ogrenci.OgrenciNumarasi,
                        BolumId = od.Ogrenci.BolumId,
                        BolumAdi = od.Ogrenci.Bolum.Adi,
                        Sinif = od.Ogrenci.Sinif
                    })
                    .ToListAsync(cancellationToken);
            }

            var students = _context.Ogrenciler.AsQueryable();
            if (request.BolumId.HasValue && request.BolumId.Value > 0)
            {
                students = students.Where(student => student.BolumId == request.BolumId.Value);
            }

            return await students
                .Select(student => new StudentDto
                {
                    Id = student.Id,
                    Adi = student.Adi,
                    Soyadi = student.Soyadi,
                    Email = student.Email,
                    OgrenciNumarasi = student.OgrenciNumarasi,
                    BolumId = student.BolumId,
                    BolumAdi = student.Bolum.Adi,
                    Sinif = student.Sinif
                })
                .ToListAsync(cancellationToken);
        }
    }
}
