// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Khách hàng đặt phòng (kiểm tra nghiệp vụ, tự tính tiền, chống trùng lịch) và xem chi tiết phiếu
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Helpers;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    // Toàn bộ controller này chỉ dành cho khách hàng đã đăng nhập
    [YeuCauVaiTro(VaiTro.KhachHang)]
    public class DatPhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatPhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== Form đặt phòng =====
        [HttpGet]
        public async Task<IActionResult> Create(int maPhong)
        {
            var khachHang = await LayKhachHangDangNhapAsync();
            if (khachHang == null)
            {
                TempData["Error"] = "Bạn cần hoàn thiện hồ sơ khách hàng trước khi đặt phòng.";
                return RedirectToAction("ThongTinCaNhan", "KhachHang");
            }

            var phong = await _context.Phongs
                .AsNoTracking()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == maPhong);
            if (phong == null) return NotFound();

            if (phong.TrangThai == TrangThaiPhong.TamNgung)
            {
                TempData["Error"] = "Phòng này đang tạm ngừng phục vụ, không thể đặt.";
                return RedirectToAction("Index", "Home");
            }

            ViewData["Phong"] = phong;
            return View(new DatPhongViewModel { MaPhong = maPhong });
        }

        // ===== Xử lý đặt phòng =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatPhongViewModel model)
        {
            // 1. Khách hàng tồn tại và đang hoạt động
            var khachHang = await LayKhachHangDangNhapAsync();
            if (khachHang == null)
            {
                TempData["Error"] = "Bạn cần hoàn thiện hồ sơ khách hàng trước khi đặt phòng.";
                return RedirectToAction("ThongTinCaNhan", "KhachHang");
            }
            if (!khachHang.TrangThai)
            {
                ModelState.AddModelError(string.Empty,
                    "Hồ sơ khách hàng của bạn đang bị khóa, không thể đặt phòng.");
            }

            // 2. Phòng tồn tại và đang phục vụ
            var phong = await _context.Phongs
                .AsNoTracking()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == model.MaPhong);
            if (phong == null) return NotFound();

            if (phong.TrangThai == TrangThaiPhong.TamNgung)
            {
                ModelState.AddModelError(string.Empty,
                    "Phòng này đang tạm ngừng phục vụ, không thể đặt.");
            }

            // 3 và 4. Kiểm tra ngày nhận, ngày trả
            if (model.NgayNhanDuKien.HasValue && model.NgayTraDuKien.HasValue)
            {
                DateTime ngayNhanKt = model.NgayNhanDuKien.Value.Date;
                DateTime ngayTraKt = model.NgayTraDuKien.Value.Date;

                if (ngayNhanKt < DateTime.Today)
                {
                    ModelState.AddModelError(nameof(model.NgayNhanDuKien),
                        "Ngày nhận phòng không được trước ngày hôm nay.");
                }
                if (ngayTraKt <= ngayNhanKt)
                {
                    ModelState.AddModelError(nameof(model.NgayTraDuKien),
                        "Ngày trả phòng phải sau ngày nhận phòng.");
                }
            }

            // 5. Số người không vượt sức chứa của loại phòng
            if (model.SoNguoi.HasValue && phong.LoaiPhong != null
                && model.SoNguoi.Value > phong.LoaiPhong.SoNguoiToiDa)
            {
                ModelState.AddModelError(nameof(model.SoNguoi),
                    $"Loại phòng \"{phong.LoaiPhong.TenLoaiPhong}\" chỉ chứa tối đa {phong.LoaiPhong.SoNguoiToiDa} người.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Phong"] = phong;
                return View(model);
            }

            DateTime ngayNhan = model.NgayNhanDuKien!.Value.Date;
            DateTime ngayTra = model.NgayTraDuKien!.Value.Date;

            // 6. Kiểm tra trùng lịch VÀ lưu phiếu trong cùng một transaction.
            // Mức Serializable: hai người đặt cùng lúc không thể cùng lọt qua bước kiểm tra.
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            if (await _context.CoTrungLichAsync(phong.MaPhong, ngayNhan, ngayTra))
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Phòng đã có người đặt trong khoảng thời gian này. Vui lòng chọn ngày khác.");
                ViewData["Phong"] = phong;
                return View(model);
            }

            // Hệ thống TỰ tính, không lấy bất kỳ giá trị tiền nào từ form
            int soDem = (ngayTra - ngayNhan).Days;
            decimal donGiaDem = phong.DonGiaDem;      // đọc từ DB
            decimal thanhTien = soDem * donGiaDem;

            var phieu = new PhieuDatPhong
            {
                MaKhachHang = khachHang.MaKhachHang,  // lấy từ Session, không từ form
                NgayDat = DateTime.Now,
                NgayNhanDuKien = ngayNhan,
                NgayTraDuKien = ngayTra,
                TongTienDuKien = thanhTien,           // phiếu 1 phòng nên tổng = thành tiền
                TrangThai = TrangThaiPhieu.ChoXacNhan
            };
            phieu.ChiTietDatPhongs.Add(new ChiTietDatPhong
            {
                MaPhong = phong.MaPhong,
                DonGiaDem = donGiaDem,
                SoDem = soDem,
                ThanhTien = thanhTien
            });

            _context.PhieuDatPhongs.Add(phieu);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đặt phòng thành công! Phiếu của bạn đang chờ khách sạn xác nhận.";
            return RedirectToAction(nameof(Details), new { id = phieu.MaPhieuDat });
        }

        // ===== Chi tiết một phiếu (chỉ xem được phiếu của chính mình) =====
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var khachHang = await LayKhachHangDangNhapAsync();
            if (khachHang == null) return NotFound();

            // Điều kiện MaKhachHang nằm ngay trong truy vấn: phiếu của người khác
            // cũng trả về NotFound, không tiết lộ phiếu có tồn tại hay không.
            var phieu = await _context.PhieuDatPhongs
                .AsNoTracking()
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(c => c.Phong)
                        .ThenInclude(ph => ph!.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id
                                       && p.MaKhachHang == khachHang.MaKhachHang);

            if (phieu == null) return NotFound();

            return View(phieu);
        }

        // Lấy hồ sơ khách hàng của người đang đăng nhập (null nếu chưa có hồ sơ)
        private async Task<KhachHang?> LayKhachHangDangNhapAsync()
        {
            int maTaiKhoan = HttpContext.Session.LayMaTaiKhoan()!.Value;
            return await _context.KhachHangs
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);
        }
    }
}