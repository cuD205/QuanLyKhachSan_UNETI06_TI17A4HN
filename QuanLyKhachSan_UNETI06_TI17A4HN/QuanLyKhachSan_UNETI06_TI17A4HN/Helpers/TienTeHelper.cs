// Sinh viên: [Nguyen Thanh Liem] - MSSV: [23103100281]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Định dạng tiền Việt Nam dùng chung cho các View (350000 -> 350.000 ₫)
using System.Globalization;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    public static class TienTeHelper
    {
        private static readonly CultureInfo ViVN = new CultureInfo("vi-VN");

        public static string ToTien(this decimal soTien)
        {
            return soTien.ToString("N0", ViVN) + " ₫";
        }
    }
}
