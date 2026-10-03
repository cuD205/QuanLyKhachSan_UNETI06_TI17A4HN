// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo hằng số vai trò, trạng thái phòng, trạng thái phiếu đặt
namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public static class VaiTro
    {
        public const string Admin = "Admin";
        public const string KhachHang = "KhachHang";
    }

    public static class TrangThaiPhong
    {
        public const string SanSang = "SanSang";
        public const string DangSuDung = "DangSuDung";
        public const string TamNgung = "TamNgung";
    }

    // Luồng: ChoXacNhan -> DaXacNhan -> DaNhanPhong -> DaTraPhong (hoặc DaHuy)
    public static class TrangThaiPhieu
    {
        public const string ChoXacNhan = "ChoXacNhan";
        public const string DaXacNhan = "DaXacNhan";
        public const string DaNhanPhong = "DaNhanPhong";
        public const string DaTraPhong = "DaTraPhong";
        public const string DaHuy = "DaHuy";
    }
}
