// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Module 4 - Quản lý phiếu đặt phòng, nhận/trả phòng, Dashboard, thống kê
// Nội dung: ViewModel chứa 8 chỉ số của trang Dashboard
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
    }
}
