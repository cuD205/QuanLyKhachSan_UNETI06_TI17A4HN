// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Quản lý khách hàng (Admin): danh sách, tìm kiếm, chi tiết, khóa/mở khóa
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Helpers;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== PHẦN DÀNH CHO ADMIN =====

        // Danh sách + tìm kiếm theo họ tên / SĐT / email / CCCD
        [YeuCauVaiTro(VaiTro.Admin)]
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            // AsNoTracking: chỉ đọc, không cần EF theo dõi thay đổi
            var query = _context.KhachHangs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                // Contains được EF dịch thành LIKE '%từ khóa%' trong SQL
                query = query.Where(k => k.HoTen.Contains(tuKhoa)
                                      || k.SoDienThoai.Contains(tuKhoa)
                                      || k.Email.Contains(tuKhoa)
                                      || k.CCCD.Contains(tuKhoa));
            }

            var danhSach = await query
                .OrderByDescending(k => k.NgayDangKy)
                .ToListAsync();

            ViewData["TuKhoa"] = tuKhoa;
            return View(danhSach);
        }

        // Chi tiết một khách hàng kèm các phiếu đặt phòng của họ
        [YeuCauVaiTro(VaiTro.Admin)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .Include(k => k.TaiKhoan)          // lấy tên đăng nhập
                .Include(k => k.PhieuDatPhongs)    // lấy các phiếu đặt
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null) return NotFound();

            return View(khachHang);
        }

        // Khóa / mở khóa khách hàng (đảo trạng thái)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauVaiTro(VaiTro.Admin)]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null) return NotFound();

            khachHang.TrangThai = !khachHang.TrangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] = khachHang.TrangThai
                ? $"Đã mở khóa khách hàng {khachHang.HoTen}."
                : $"Đã khóa khách hàng {khachHang.HoTen}.";

            return RedirectToAction(nameof(Index));
        }
        // ===== PHẦN DÀNH CHO KHÁCH HÀNG =====

        // Xem thông tin cá nhân của chính mình
        [HttpGet]
        [YeuCauVaiTro(VaiTro.KhachHang)]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            // Mã tài khoản lấy từ Session, KHÔNG nhận từ URL hay form
            int maTaiKhoan = HttpContext.Session.LayMaTaiKhoan()!.Value;

            var khachHang = await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

            var model = new ThongTinCaNhanViewModel();

            if (khachHang != null)
            {
                model.HoTen = khachHang.HoTen;
                model.NgaySinh = khachHang.NgaySinh;
                model.GioiTinh = khachHang.GioiTinh;
                model.SoDienThoai = khachHang.SoDienThoai;
                model.Email = khachHang.Email;
                model.CCCD = khachHang.CCCD;
                model.DiaChi = khachHang.DiaChi;
            }
            else
            {
                // Chưa có hồ sơ: gợi ý sẵn họ tên và email từ tài khoản
                var taiKhoan = await _context.TaiKhoans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);
                if (taiKhoan != null)
                {
                    model.HoTen = taiKhoan.HoTen;
                    model.Email = taiKhoan.Email;
                }
            }

            DatCoBaoChoView(khachHang);
            return View(model);
        }

        // Lưu thông tin cá nhân (tạo hồ sơ mới nếu chưa có)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [YeuCauVaiTro(VaiTro.KhachHang)]
        public async Task<IActionResult> ThongTinCaNhan(ThongTinCaNhanViewModel model)
        {
            int maTaiKhoan = HttpContext.Session.LayMaTaiKhoan()!.Value;

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

            // Khách đang bị khóa thì không được cập nhật
            if (khachHang != null && !khachHang.TrangThai)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản khách hàng đang bị khóa, không thể cập nhật thông tin.");
            }

            // CCCD phải duy nhất (DB có unique index), kiểm tra trước để báo lỗi dễ hiểu
            if (!string.IsNullOrWhiteSpace(model.CCCD))
            {
                int maKhachHangHienTai = khachHang?.MaKhachHang ?? 0;
                bool trungCccd = await _context.KhachHangs
                    .AnyAsync(k => k.CCCD == model.CCCD && k.MaKhachHang != maKhachHangHienTai);
                if (trungCccd)
                {
                    ModelState.AddModelError(nameof(model.CCCD),
                        "CCCD này đã được đăng ký cho khách hàng khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                DatCoBaoChoView(khachHang);
                return View(model);
            }

            // Chưa có hồ sơ thì tạo mới; các trường hệ thống do server gán
            if (khachHang == null)
            {
                khachHang = new KhachHang
                {
                    MaTaiKhoan = maTaiKhoan,
                    NgayDangKy = DateTime.Now,
                    TrangThai = true
                };
                _context.KhachHangs.Add(khachHang);
            }

            // Chỉ cập nhật đúng 7 trường được phép
            khachHang.HoTen = model.HoTen;
            khachHang.NgaySinh = model.NgaySinh!.Value;
            khachHang.GioiTinh = model.GioiTinh;
            khachHang.SoDienThoai = model.SoDienThoai;
            khachHang.Email = model.Email;
            khachHang.CCCD = model.CCCD;
            khachHang.DiaChi = model.DiaChi;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }

        // Báo cho View biết: đã có hồ sơ chưa, có đang bị khóa không
        private void DatCoBaoChoView(KhachHang? khachHang)
        {
            ViewData["DaCoHoSo"] = khachHang != null;
            ViewData["BiKhoa"] = khachHang != null && !khachHang.TrangThai;
        }
    }

}
