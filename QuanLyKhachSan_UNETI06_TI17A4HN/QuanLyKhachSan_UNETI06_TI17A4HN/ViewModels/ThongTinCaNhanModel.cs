// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: ViewModel cho form khách hàng xem/sửa thông tin cá nhân
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels
{
    // Chỉ chứa các trường khách hàng được phép sửa.
    // Không có MaTaiKhoan, TrangThai, NgayDangKy để tránh bị sửa lén qua form.
    public class ThongTinCaNhanViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        [NgaySinhHopLe]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số, bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "CCCD không được để trống")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD phải gồm 12 chữ số")]
        [Display(Name = "CCCD")]
        public string CCCD { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }
}