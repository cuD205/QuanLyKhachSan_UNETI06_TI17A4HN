// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: ViewModel cho trang thống kê (6 nhóm số liệu tính bằng LINQ)
namespace QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels
{
    // Một dòng thống kê dạng "nhãn - số lượng" (số phòng theo loại, số lượt đặt theo phòng...)
    public class ThongKeItem
    {
        public string Nhan { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    // Một dòng doanh thu theo tháng
    public class ThongKeDoanhThuThang
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public int SoPhieu { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class ThongKeViewModel
    {
        public List<ThongKeItem> SoPhongTheoLoai { get; set; } = new();
        public List<ThongKeItem> SoPhieuTheoTrangThai { get; set; } = new();
        public List<ThongKeItem> SoLuotTheoPhong { get; set; } = new();
        public List<ThongKeItem> SoLuotTheoLoaiPhong { get; set; } = new();   // xếp hạng loại phòng
        public List<ThongKeItem> LoaiPhongNhieuNhat { get; set; } = new();    // loại đứng đầu (có thể hòa)
        public List<ThongKeItem> SoLuotTheoKhachHang { get; set; } = new();
        public List<ThongKeDoanhThuThang> DoanhThuTheoThang { get; set; } = new();
    }
}
