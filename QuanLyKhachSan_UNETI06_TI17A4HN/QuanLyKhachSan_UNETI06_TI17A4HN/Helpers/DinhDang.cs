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

        // Class nhãn trạng thái dạng viên thuốc.
        // Muốn đổi màu nhãn cho khớp theme nhóm thì CHỈ cần sửa 5 hằng này.
        private const string NhanVang = "badge rounded-pill bg-warning text-dark";
        private const string NhanXanhDuong = "badge rounded-pill bg-info text-dark";
        private const string NhanXanhLa = "badge rounded-pill bg-success";
        private const string NhanDo = "badge rounded-pill bg-danger";

        // Ví dụ: 350000 -> "350.000 ₫"
        public static string Tien(decimal soTien)
        {
            return string.Format(ViVN, "{0:N0} ₫", soTien);
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

        // Class đầy đủ của nhãn trạng thái phiếu (dùng: class="@DinhDang.ClassTrangThaiPhieu(...)")
        public static string ClassTrangThaiPhieu(string trangThai)
        {
            return trangThai switch
            {
                TrangThaiPhieu.ChoXacNhan => NhanVang,
                TrangThaiPhieu.DaXacNhan => NhanXanhDuong,
                TrangThaiPhieu.DaNhanPhong => NhanVang,
                TrangThaiPhieu.DaTraPhong => NhanXanhLa,
                TrangThaiPhieu.DaHuy => NhanDo,
                _ => "badge rounded-pill bg-secondary"
            };
        }

        // Trạng thái khách hàng (bool: true = hoạt động)
        public static string TenTrangThaiKhach(bool hoatDong)
        {
            return hoatDong ? "Hoạt động" : "Bị khóa";
        }

        public static string ClassTrangThaiKhach(bool hoatDong)
        {
            return hoatDong ? NhanXanhLa : NhanDo;
        }
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