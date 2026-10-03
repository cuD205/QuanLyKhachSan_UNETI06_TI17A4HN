// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Khách hàng
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime NgaySinh { get; set; }

        [StringLength(10)]
        public string GioiTinh { get; set; } = string.Empty;

        [Required, RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0")]
        [StringLength(15)]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải gồm 12 chữ số")]
        [StringLength(12)]
        public string CCCD { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DiaChi { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public bool TrangThai { get; set; } = true;

        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<PhieuDatPhong> PhieuDatPhongs { get; set; } = new List<PhieuDatPhong>();
    }
}
