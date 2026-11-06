// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: ViewModel chứa 8 chỉ số và dữ liệu 2 biểu đồ của trang Dashboard
namespace QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels
{
    public class DashboardViewModel
    {
        public int TongLoaiPhong { get; set; }
        public int TongPhong { get; set; }
        public int TongKhachHang { get; set; }
        public int PhongDangSuDung { get; set; }

        public int TongPhieuDatPhong { get; set; }
        public int PhieuChoXacNhan { get; set; }
        public int PhieuDaXacNhan { get; set; }
        public int PhieuDaTraPhong { get; set; }

        // Dữ liệu cho biểu đồ Chart.js (dùng lại ThongKeItem và ThongKeDoanhThuThang trong ThongKeViewModel.cs)
        public List<ThongKeItem> PhieuTheoTrangThai { get; set; } = new();
        public List<ThongKeDoanhThuThang> DoanhThuTheoThang { get; set; } = new();
    }
}
