// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo DbContext, cấu hình quan hệ, khóa chính/khóa ngoại giữa các Entity
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
        public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
        public DbSet<Phong> Phongs => Set<Phong>();
        public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
        public DbSet<PhieuDatPhong> PhieuDatPhongs => Set<PhieuDatPhong>();
        public DbSet<ChiTietDatPhong> ChiTietDatPhongs => Set<ChiTietDatPhong>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<LoaiPhong>().HasIndex(l => l.TenLoaiPhong).IsUnique();
            modelBuilder.Entity<Phong>().HasIndex(p => p.SoPhong).IsUnique();
            modelBuilder.Entity<KhachHang>().HasIndex(k => k.CCCD).IsUnique();

            // TaiKhoan 1 - 1 KhachHang
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Restrict);

            // LoaiPhong 1 - n Phong
            modelBuilder.Entity<Phong>()
                .HasOne(p => p.LoaiPhong)
                .WithMany(l => l.Phongs)
                .HasForeignKey(p => p.MaLoaiPhong)
                .OnDelete(DeleteBehavior.Restrict);

            // KhachHang 1 - n PhieuDatPhong
            modelBuilder.Entity<PhieuDatPhong>()
                .HasOne(p => p.KhachHang)
                .WithMany(k => k.PhieuDatPhongs)
                .HasForeignKey(p => p.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // ChiTietDatPhong: khóa chính kép, nối PhieuDatPhong và Phong
            modelBuilder.Entity<ChiTietDatPhong>()
                .HasKey(c => new { c.MaPhieuDat, c.MaPhong });

            modelBuilder.Entity<ChiTietDatPhong>()
                .HasOne(c => c.PhieuDatPhong)
                .WithMany(p => p.ChiTietDatPhongs)
                .HasForeignKey(c => c.MaPhieuDat)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDatPhong>()
                .HasOne(c => c.Phong)
                .WithMany(p => p.ChiTietDatPhongs)
                .HasForeignKey(c => c.MaPhong)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
