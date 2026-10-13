// Sinh viên: [Họ và tên] - MSSV: [Mã sinh viên]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Lưu / đọc thông tin đăng nhập trong Session (Mã tài khoản, Họ tên, Vai trò) - cả nhóm dùng chung
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    public static class PhienDangNhap
    {
        public const string KeyMaTaiKhoan = "MaTaiKhoan";
        public const string KeyHoTen = "HoTen";
        public const string KeyVaiTro = "VaiTro";

        // Lưu tối thiểu Mã tài khoản, Họ tên, Vai trò vào Session (theo yêu cầu đề bài)
        public static void LuuDangNhap(this ISession session, TaiKhoan taiKhoan)
        {
            session.SetInt32(KeyMaTaiKhoan, taiKhoan.MaTaiKhoan);
            session.SetString(KeyHoTen, taiKhoan.HoTen);
            session.SetString(KeyVaiTro, taiKhoan.VaiTro);
        }

        public static bool DaDangNhap(this ISession session)
        {
            return session.GetInt32(KeyMaTaiKhoan).HasValue;
        }

        public static int? LayMaTaiKhoan(this ISession session)
        {
            return session.GetInt32(KeyMaTaiKhoan);
        }

        public static string? LayHoTen(this ISession session)
        {
            return session.GetString(KeyHoTen);
        }

        public static string? LayVaiTro(this ISession session)
        {
            return session.GetString(KeyVaiTro);
        }

        public static bool LaAdmin(this ISession session)
        {
            return session.GetString(KeyVaiTro) == VaiTro.Admin;
        }

        public static bool LaKhachHang(this ISession session)
        {
            return session.GetString(KeyVaiTro) == VaiTro.KhachHang;
        }
    }
}
