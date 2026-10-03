// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Phiếu đặt phòng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class PhieuDatPhong
    {
        [Key]
        public int MaPhieuDat { get; set; }

        public int MaKhachHang { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime NgayNhanDuKien { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayTraDuKien { get; set; }

        // Hệ thống tự tính, không cho người dùng nhập
        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTienDuKien { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = TrangThaiPhieu.ChoXacNhan;

        public KhachHang? KhachHang { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = new List<ChiTietDatPhong>();
    }
}
