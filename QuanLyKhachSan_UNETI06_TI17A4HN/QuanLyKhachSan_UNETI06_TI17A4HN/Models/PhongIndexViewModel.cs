// Họ và tên: Diện | Mã sinh viên: [MSSV của bạn]
// Module 2: Quản lý và tra cứu Phòng
// Nội dung: ViewModel cho trang danh sách Phòng
using Microsoft.AspNetCore.Mvc.Rendering;
namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class PhongIndexViewModel
    {
        public List<Phong> Phongs { get; set; } = new();
        public string? Search { get; set; }
        // Điều kiện lọc đang dùng
        public int? MaLoaiPhong { get; set; }
        public int? Tang { get; set; }
        public string? TrangThai { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }

        // Dữ liệu cho các ô chọn
        public List<SelectListItem> LoaiPhongOptions { get; set; } = new();
        public List<SelectListItem> TangOptions { get; set; } = new();
        public List<SelectListItem> TrangThaiOptions { get; set; } = new();
    }


}