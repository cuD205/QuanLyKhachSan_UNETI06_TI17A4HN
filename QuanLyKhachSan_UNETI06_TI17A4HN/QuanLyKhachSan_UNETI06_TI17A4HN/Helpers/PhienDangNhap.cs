// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Hằng key Session và hàm đọc Session dùng cho Module 3
namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    // Tên các key Session. Nếu Module 1 đặt tên khác thì chỉ cần sửa tại đây.
    public static class SessionKeys
    {
        public const string MaTaiKhoan = "MaTaiKhoan";
        public const string HoTen = "HoTen";
        public const string VaiTro = "VaiTro";

        // Trang đăng nhập của Module 1 (sửa lại cho đúng khi Module 1 xong)
        public const string DuongDanDangNhap = "/TaiKhoan/DangNhap";
    }

    public static class PhienDangNhapExtensions
    {
        // Mã tài khoản đang đăng nhập (null nếu chưa đăng nhập)
        public static int? LayMaTaiKhoan(this ISession session)
        {
            return session.GetInt32(SessionKeys.MaTaiKhoan);
        }

        // Vai trò đang đăng nhập (null nếu chưa đăng nhập)
        public static string? LayVaiTro(this ISession session)
        {
            return session.GetString(SessionKeys.VaiTro);
        }
    }
}