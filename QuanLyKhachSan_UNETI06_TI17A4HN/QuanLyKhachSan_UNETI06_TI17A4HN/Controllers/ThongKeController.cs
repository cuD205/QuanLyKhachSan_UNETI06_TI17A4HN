// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: Dashboard 8 chỉ số và trang thống kê 6 mục bằng LINQ (Count, Sum, GroupBy, OrderBy, OrderByDescending) - chỉ Admin
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Filters;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    // Kiểm tra quyền ở Controller: chỉ Admin được xem Dashboard và thống kê
    [PhanQuyen(VaiTro.Admin)]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================ BƯỚC 3: DASHBOARD ============================

        // GET: ThongKe/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Mỗi chỉ số là một truy vấn COUNT trên SQL Server (không tải dữ liệu về rồi mới đếm).
            // DbContext không chạy song song được nên các truy vấn await lần lượt.
            var model = new DashboardViewModel
            {
                TongLoaiPhong = await _context.LoaiPhongs.CountAsync(),
                TongPhong = await _context.Phongs.CountAsync(),
                TongKhachHang = await _context.KhachHangs.CountAsync(),
                PhongDangSuDung = await _context.Phongs
                    .CountAsync(p => p.TrangThai == TrangThaiPhong.DangSuDung),

                TongPhieuDatPhong = await _context.PhieuDatPhongs.CountAsync(),
                PhieuChoXacNhan = await _context.PhieuDatPhongs
                    .CountAsync(p => p.TrangThai == TrangThaiPhieu.ChoXacNhan),
                PhieuDaXacNhan = await _context.PhieuDatPhongs
                    .CountAsync(p => p.TrangThai == TrangThaiPhieu.DaXacNhan),
                PhieuDaTraPhong = await _context.PhieuDatPhongs
                    .CountAsync(p => p.TrangThai == TrangThaiPhieu.DaTraPhong)
            };

            return View(model);
        }

        // ============================ BƯỚC 4: THỐNG KÊ ============================

        // GET: ThongKe
        public async Task<IActionResult> Index()
        {
            // Quy ước: "lượt đặt" không tính phiếu đã hủy (phiếu hủy không phát sinh lượt lưu trú).
            string daHuy = TrangThaiPhieu.DaHuy;

            // 1. Số phòng theo từng loại (loại chưa có phòng vẫn hiện với số 0)
            var soPhongTheoLoai = await _context.LoaiPhongs
                .Select(l => new ThongKeItem { Nhan = l.TenLoaiPhong, SoLuong = l.Phongs.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Nhan)
                .ToListAsync();

            // 2. Số phiếu theo từng trạng thái (GroupBy + Count), rồi bù trạng thái chưa có phiếu = 0
            var theoTrangThai = await _context.PhieuDatPhongs
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            var thuTuTrangThai = new[]
            {
                TrangThaiPhieu.ChoXacNhan, TrangThaiPhieu.DaXacNhan, TrangThaiPhieu.DaNhanPhong,
                TrangThaiPhieu.DaTraPhong, TrangThaiPhieu.DaHuy
            };
            var soPhieuTheoTrangThai = thuTuTrangThai
                .Select(tt => new ThongKeItem
                {
                    Nhan = TenTrangThaiPhieu(tt),
                    SoLuong = theoTrangThai.FirstOrDefault(x => x.TrangThai == tt)?.SoLuong ?? 0
                })
                .ToList();

            // 3. Số lượt đặt theo từng phòng (GroupBy theo số phòng, nhiều nhất lên đầu)
            var soLuotTheoPhong = await _context.ChiTietDatPhongs
                .Where(c => c.PhieuDatPhong!.TrangThai != daHuy)
                .GroupBy(c => c.Phong!.SoPhong)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Nhan)
                .ToListAsync();

            // 4. Loại phòng có số lượt đặt nhiều nhất: xếp hạng mọi loại, rồi lấy các loại bằng điểm cao nhất
            var soLuotTheoLoaiPhong = await _context.ChiTietDatPhongs
                .Where(c => c.PhieuDatPhong!.TrangThai != daHuy)
                .GroupBy(c => c.Phong!.LoaiPhong!.TenLoaiPhong)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Nhan)
                .ToListAsync();

            int cao = soLuotTheoLoaiPhong.Count == 0 ? 0 : soLuotTheoLoaiPhong.Max(x => x.SoLuong);
            var loaiPhongNhieuNhat = soLuotTheoLoaiPhong
                .Where(x => cao > 0 && x.SoLuong == cao)
                .ToList();

            // 5. Số lượt đặt theo từng khách hàng (gộp theo mã khách để hai người trùng tên không bị gộp nhầm)
            var soLuotTheoKhachHang = await _context.PhieuDatPhongs
                .Where(p => p.TrangThai != daHuy)
                .GroupBy(p => new { p.MaKhachHang, p.KhachHang!.HoTen })
                .Select(g => new ThongKeItem { Nhan = g.Key.HoTen, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ThenBy(x => x.Nhan)
                .ToListAsync();

            // 6. Doanh thu theo tháng, chỉ tính phiếu ĐÃ TRẢ PHÒNG (GroupBy năm-tháng + Sum)
            // Hệ thống chưa lưu ngày trả thực tế nên lấy tháng của ngày trả dự kiến.
            var doanhThuTheoThang = await _context.PhieuDatPhongs
                .Where(p => p.TrangThai == TrangThaiPhieu.DaTraPhong)
                .GroupBy(p => new { p.NgayTraDuKien.Year, p.NgayTraDuKien.Month })
                .Select(g => new ThongKeDoanhThuThang
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoPhieu = g.Count(),
                    DoanhThu = g.Sum(p => p.TongTienDuKien)
                })
                .OrderBy(x => x.Nam)
                .ThenBy(x => x.Thang)
                .ToListAsync();

            var model = new ThongKeViewModel
            {
                SoPhongTheoLoai = soPhongTheoLoai,
                SoPhieuTheoTrangThai = soPhieuTheoTrangThai,
                SoLuotTheoPhong = soLuotTheoPhong,
                SoLuotTheoLoaiPhong = soLuotTheoLoaiPhong,
                LoaiPhongNhieuNhat = loaiPhongNhieuNhat,
                SoLuotTheoKhachHang = soLuotTheoKhachHang,
                DoanhThuTheoThang = doanhThuTheoThang
            };

            return View(model);
        }

        private static string TenTrangThaiPhieu(string trangThai) => trangThai switch
        {
            TrangThaiPhieu.ChoXacNhan => "Chờ xác nhận",
            TrangThaiPhieu.DaXacNhan => "Đã xác nhận",
            TrangThaiPhieu.DaNhanPhong => "Đã nhận phòng",
            TrangThaiPhieu.DaTraPhong => "Đã trả phòng",
            TrangThaiPhieu.DaHuy => "Đã hủy",
            _ => trangThai
        };
    }
}
