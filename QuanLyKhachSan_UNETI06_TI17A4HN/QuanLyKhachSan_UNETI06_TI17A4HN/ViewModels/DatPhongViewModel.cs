// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: ViewModel cho form đặt phòng của khách hàng
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels
{
    // Chỉ chứa dữ liệu khách được phép nhập.
    // KHÔNG có đơn giá, số đêm, thành tiền, tổng tiền: do hệ thống tự tính.
    public class DatPhongViewModel
    {
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime? NgayNhanDuKien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả phòng")]
        public DateTime? NgayTraDuKien { get; set; }

        // Chỉ dùng để kiểm tra sức chứa, không lưu vào DB (phiếu không có cột này)
        [Required(ErrorMessage = "Vui lòng nhập số người")]
        [Range(1, 20, ErrorMessage = "Số người phải từ 1 đến 20")]
        [Display(Name = "Số người")]
        public int? SoNguoi { get; set; } = 1;
    }
}