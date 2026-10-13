// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Hàm kiểm tra trùng lịch đặt phòng (Module 4 có thể dùng lại)
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    public static class DatPhongExtensions
    {
        // Các trạng thái phiếu còn "giữ chỗ" phòng (đã hủy hoặc đã trả thì không còn giữ)
        private static readonly string[] TrangThaiGiuCho =
        {
            TrangThaiPhieu.ChoXacNhan,
            TrangThaiPhieu.DaXacNhan,
            TrangThaiPhieu.DaNhanPhong
        };

        // Trả về true nếu phòng đã bị đặt trong khoảng [ngayNhan, ngayTra).
        // maPhieuLoaiTru: bỏ qua một phiếu (dùng khi kiểm tra lại chính phiếu đó).
        // Cách dùng ở nơi khác: await _context.CoTrungLichAsync(maPhong, nhan, tra);
        public static async Task<bool> CoTrungLichAsync(
            this ApplicationDbContext context,
            int maPhong,
            DateTime ngayNhan,
            DateTime ngayTra,
            int? maPhieuLoaiTru = null)
        {
            DateTime nhan = ngayNhan.Date;
            DateTime tra = ngayTra.Date;
            int maLoaiTru = maPhieuLoaiTru ?? 0;

            // Hai khoảng thời gian trùng nhau khi: nhanCu < traMoi VÀ traCu > nhanMoi.
            // So sánh theo NGÀY (.Date) nên ngày trả của người này có thể là ngày nhận của người kia.
            return await context.PhieuDatPhongs.AnyAsync(p =>
                TrangThaiGiuCho.Contains(p.TrangThai)
                && p.MaPhieuDat != maLoaiTru
                && p.NgayNhanDuKien.Date < tra
                && p.NgayTraDuKien.Date > nhan
                && p.ChiTietDatPhongs.Any(c => c.MaPhong == maPhong));
        }
    }
}