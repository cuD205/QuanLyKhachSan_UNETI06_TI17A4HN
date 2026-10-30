// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: ViewModel cho trang danh sách phiếu đặt (điều kiện tìm kiếm/lọc + kết quả)
using System.ComponentModel.DataAnnotations;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels
{
    public class PhieuDatPhongIndexViewModel
    {
        // ----- Điều kiện lọc (giữ lại để hiển thị lại trên form) -----
        public string? TuKhoa { get; set; }          // tên khách hàng
        public string? TrangThai { get; set; }       // một trong TrangThaiPhieu.*

        [DataType(DataType.Date)]
        public DateTime? TuNgay { get; set; }        // ngày nhận dự kiến từ

        [DataType(DataType.Date)]
        public DateTime? DenNgay { get; set; }       // ngày nhận dự kiến đến

        // ----- Kết quả -----
        public List<PhieuDatPhong> DanhSach { get; set; } = new();
    }
}
