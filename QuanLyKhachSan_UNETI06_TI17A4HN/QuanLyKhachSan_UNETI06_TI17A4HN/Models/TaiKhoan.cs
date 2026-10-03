// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Tài khoản
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty; // lưu dạng hash

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required, EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string VaiTro { get; set; } = Models.VaiTro.KhachHang;

        public bool TrangThai { get; set; } = true; // true = hoạt động, false = khóa

        public KhachHang? KhachHang { get; set; }
    }
}
