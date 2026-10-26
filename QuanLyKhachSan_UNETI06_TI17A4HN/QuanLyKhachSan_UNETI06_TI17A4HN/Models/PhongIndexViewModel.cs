// Họ và tên: [Đoàn Diện]
// Mã sinh viên: [MSSV]
// Nội dung thực hiện: Model cho trang danh sách phòng (điều kiện tìm kiếm, lọc, sắp xếp, phân trang)
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class PhongIndexViewModel
    {
        public List<Phong> Phongs { get; set; } = new();

        // Điều kiện tìm kiếm, lọc, sắp xếp đang dùng (phải giữ nguyên khi chuyển trang)
        public string? Search { get; set; }
        public int? MaLoaiPhong { get; set; }
        public int? Tang { get; set; }
        public string? TrangThai { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string SapXep { get; set; } = "sophong_tang";

        // Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; } = 1;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;

        // Dữ liệu cho các ô chọn
        public List<SelectListItem> LoaiPhongOptions { get; set; } = new();
        public List<SelectListItem> TangOptions { get; set; } = new();
        public List<SelectListItem> TrangThaiOptions { get; set; } = new();
        public List<SelectListItem> SapXepOptions { get; set; } = new();
        public List<SelectListItem> PageSizeOptions { get; set; } = new();
    }
}