// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Chi tiết đặt phòng
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    // Khóa chính kép (MaPhieuDat, MaPhong) được cấu hình trong ApplicationDbContext
    public class ChiTietDatPhong
    {
        public int MaPhieuDat { get; set; }
        public int MaPhong { get; set; }

        // Sao chép đơn giá tại thời điểm đặt để giá cũ không đổi khi phòng đổi giá
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaDem { get; set; }

        public int SoDem { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThanhTien { get; set; } // = SoDem * DonGiaDem, hệ thống tự tính

        public PhieuDatPhong? PhieuDatPhong { get; set; }
        public Phong? Phong { get; set; }
    }
}
