// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Hàm định dạng tiền, ngày và hiển thị trạng thái phiếu đặt phòng
using System.Globalization;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    public static class DinhDang
    {
        private static readonly CultureInfo ViVN = new CultureInfo("vi-VN");

        public static string Tien(decimal soTien)
        {
            return string.Format(ViVN, "{0:N0} đ", soTien);
        }

        public static string Ngay(DateTime ngay)
        {
            return ngay.ToString("dd/MM/yyyy");
        }

        // Mã trong DB -> chữ hiển thị
        public static string TenTrangThaiPhieu(string trangThai)
        {
            return trangThai switch
            {
                TrangThaiPhieu.ChoXacNhan => "Chờ xác nhận",
                TrangThaiPhieu.DaXacNhan => "Đã xác nhận",
                TrangThaiPhieu.DaNhanPhong => "Đã nhận phòng",
                TrangThaiPhieu.DaTraPhong => "Đã trả phòng",
                TrangThaiPhieu.DaHuy => "Đã hủy",
                _ => trangThai
            };
        }

        // Màu badge Bootstrap theo trạng thái
        public static string MauTrangThaiPhieu(string trangThai)
        {
            return trangThai switch
            {
                TrangThaiPhieu.ChoXacNhan => "warning text-dark",
                TrangThaiPhieu.DaXacNhan => "info text-dark",
                TrangThaiPhieu.DaNhanPhong => "primary",
                TrangThaiPhieu.DaTraPhong => "success",
                TrangThaiPhieu.DaHuy => "secondary",
                _ => "light text-dark"
            };
        }
    }
}