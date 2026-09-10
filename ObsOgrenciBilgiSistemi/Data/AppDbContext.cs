using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ObsOgrenciBilgiSistemi.Entities;
using ObsOgrenciBilgiSistemi.Models;

namespace ObsOgrenciBilgiSistemi.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Veritabanı tabloları
        public DbSet<Ogrenci> Ogrenciler { get; set; }
        public DbSet<Akademisyen> Akademisyenler { get; set; }
        public DbSet<Bolum> Bolumler { get; set; }
        public DbSet<Ders> Dersler { get; set; }
        public DbSet<OgrenciDers> OgrenciDersler { get; set; }
        public DbSet<Not> Notlar { get; set; }
        public DbSet<Devamsizlik> Devamsizliklar { get; set; }
        public DbSet<Duyuru> Duyurular { get; set; }
        public DbSet<DersTalebi> DersTalepleri { get; set; }
        public DbSet<Bildirim> Bildirimler { get; set; }
        public DbSet<DersKayitTalebi> DersKayitTalepleri { get; set; }
        public DbSet<DersKayitTalepDersi> DersKayitTalepDersleri { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<OgrenciDers>()
                .HasOne(od => od.Ogrenci)
                .WithMany(o => o.OgrenciDersler)
                .HasForeignKey(od => od.OgrenciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Öğrenci ve ders ilişkileri
            modelBuilder.Entity<OgrenciDers>()
                .HasOne(od => od.Ders)
                .WithMany(d => d.OgrenciDersler)
                .HasForeignKey(od => od.DersId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Not>()
                .HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Devamsızlık ilişkileri
            modelBuilder.Entity<Devamsizlik>()
                .HasOne(attendance => attendance.Student).WithMany()
                .HasForeignKey(attendance => attendance.StudentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Devamsizlik>()
                .HasOne(attendance => attendance.Ders).WithMany()
                .HasForeignKey(attendance => attendance.DersId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Devamsizlik>()
                .HasIndex(attendance => new { attendance.StudentId, attendance.DersId, attendance.DevamsizlikHaftasi })
                .IsUnique();

            // Tekil alanlar ve indeksler
            modelBuilder.Entity<Ogrenci>()
                .HasIndex(student => student.OgrenciNumarasi)
                .IsUnique();

            // Ders talebi ilişkileri
            modelBuilder.Entity<DersTalebi>()
                .HasOne(request => request.Ders)
                .WithMany()
                .HasForeignKey(request => request.DersId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DersTalebi>()
                .HasOne(request => request.Akademisyen)
                .WithMany()
                .HasForeignKey(request => request.AkademisyenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DersTalebi>()
                .HasIndex(request => new { request.DersId, request.AkademisyenId, request.Durum });

            modelBuilder.Entity<Bildirim>()
                .HasIndex(notification => new { notification.AliciEmail, notification.Okundu });

            // Bölüm danışmanlığı
            modelBuilder.Entity<Bolum>()
                .HasOne(department => department.DanismanAkademisyen)
                .WithMany()
                .HasForeignKey(department => department.DanismanAkademisyenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ders kayıt talebi ilişkileri
            modelBuilder.Entity<DersKayitTalebi>()
                .HasOne(request => request.Ogrenci).WithMany()
                .HasForeignKey(request => request.OgrenciId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DersKayitTalebi>()
                .HasOne(request => request.DanismanAkademisyen).WithMany()
                .HasForeignKey(request => request.DanismanAkademisyenId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DersKayitTalebi>()
                .HasIndex(request => new { request.OgrenciId, request.AkademikYil, request.Donem, request.Durum });
            modelBuilder.Entity<DersKayitTalepDersi>()
                .HasIndex(item => new { item.DersKayitTalebiId, item.DersId }).IsUnique();
        }
    }
}
