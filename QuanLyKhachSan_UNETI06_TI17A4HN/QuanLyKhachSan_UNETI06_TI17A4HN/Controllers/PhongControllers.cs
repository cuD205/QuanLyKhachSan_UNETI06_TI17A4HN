// Họ và tên: [Họ tên đầy đủ]
// Mã sinh viên: [MSSV]
// Nội dung thực hiện: Danh sách, chi tiết, thêm phòng, cập nhật trạng thái phòng
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    public class PhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ===== PHÂN QUYỀN (chờ Module 1 của Liêm) =====
        // Khi Liêm merge xong đăng nhập: đổi KiemTraAdmin = true và sửa SessionVaiTro
        // cho đúng tên key Session mà Liêm lưu vai trò.
        private static readonly bool KiemTraAdmin = false;
        private const string SessionVaiTro = "VaiTro";

        public PhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool LaAdmin() =>
            !KiemTraAdmin || HttpContext.Session.GetString(SessionVaiTro) == VaiTro.Admin;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.LaAdmin = LaAdmin();
            base.OnActionExecuting(context);
        }

        // ===== DANH SÁCH =====
        public async Task<IActionResult> Index(string? search, int? maLoaiPhong, int? tang,
    string? trangThai, decimal? giaTu, decimal? giaDen)
        {
            // Nếu nhập ngược khoảng giá thì đổi chỗ
            if (giaTu.HasValue && giaDen.HasValue && giaTu > giaDen)
            {
                (giaTu, giaDen) = (giaDen, giaTu);
            }
            if (string.IsNullOrWhiteSpace(trangThai) || !DanhSachTrangThai().ContainsKey(trangThai)) trangThai = null;
            search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

            var query = _context.Phongs.AsNoTracking().Include(p => p.LoaiPhong).AsQueryable();

            // Tìm kiếm
            if (search != null)
            {
                query = query.Where(p => p.SoPhong.Contains(search)
                    || (p.LoaiPhong != null && p.LoaiPhong.TenLoaiPhong.Contains(search)));
            }

            // 4 tiêu chí lọc, kết hợp được với nhau
            if (maLoaiPhong.HasValue) query = query.Where(p => p.MaLoaiPhong == maLoaiPhong.Value);
            if (tang.HasValue) query = query.Where(p => p.Tang == tang.Value);
            if (trangThai != null) query = query.Where(p => p.TrangThai == trangThai);
            if (giaTu.HasValue) query = query.Where(p => p.DonGiaDem >= giaTu.Value);
            if (giaDen.HasValue) query = query.Where(p => p.DonGiaDem <= giaDen.Value);

            var phongs = await query
                .OrderBy(p => p.SoPhong.Length).ThenBy(p => p.SoPhong)
                .ToListAsync();

            var vm = new PhongIndexViewModel
            {
                Phongs = phongs,
                Search = search,
                MaLoaiPhong = maLoaiPhong,
                Tang = tang,
                TrangThai = trangThai,
                GiaTu = giaTu,
                GiaDen = giaDen
            };

            var loaiPhongs = await _context.LoaiPhongs.AsNoTracking().OrderBy(l => l.TenLoaiPhong).ToListAsync();
            vm.LoaiPhongOptions = loaiPhongs
                .Select(l => new SelectListItem(l.TenLoaiPhong, l.MaLoaiPhong.ToString(), l.MaLoaiPhong == maLoaiPhong))
                .ToList();

            var cacTang = await _context.Phongs.AsNoTracking().Select(p => p.Tang).Distinct().OrderBy(t => t).ToListAsync();
            vm.TangOptions = cacTang
                .Select(t => new SelectListItem($"Tầng {t}", t.ToString(), t == tang))
                .ToList();

            vm.TrangThaiOptions = DanhSachTrangThai()
                .Select(kv => new SelectListItem(kv.Value, kv.Key, kv.Key == trangThai))
                .ToList();

            return View(vm);
        }

        // ===== CHI TIẾT =====
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phong = await _context.Phongs.AsNoTracking()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);
            if (phong == null) return NotFound();

            ViewBag.SoLuotDat = await _context.ChiTietDatPhongs.CountAsync(c => c.MaPhong == id);
            return View(phong);
        }

        // ===== THÊM =====
        public async Task<IActionResult> Create()
        {
            if (!LaAdmin()) return TuChoiTruyCap();
            await NapDanhSachChon(null);
            return View(new Phong());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SoPhong,MaLoaiPhong,Tang,DonGiaDem,MoTa,TrangThai")] Phong phong)
        {
            if (!LaAdmin()) return TuChoiTruyCap();

            ModelState.Remove(nameof(Phong.LoaiPhong));
            phong.SoPhong = (phong.SoPhong ?? string.Empty).Trim();
            await KiemTraNghiepVu(phong, null);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Phongs.Add(phong);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã thêm phòng {phong.SoPhong}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Phong.SoPhong), "Không lưu được. Số phòng có thể đã tồn tại.");
                }
            }

            await NapDanhSachChon(phong.MaLoaiPhong);
            return View(phong);
        }

        // ===== CẬP NHẬT TRẠNG THÁI =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai, string? returnUrl)
        {
            if (!LaAdmin()) return TuChoiTruyCap();

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return NotFound();

            if (trangThai != TrangThaiPhong.SanSang && trangThai != TrangThaiPhong.TamNgung)
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
            }
            else if (phong.TrangThai == TrangThaiPhong.DangSuDung)
            {
                // Trạng thái này do Module 4 cập nhật khi nhận/trả phòng
                TempData["Error"] = $"Phòng {phong.SoPhong} đang có khách, không thể đổi trạng thái. Trạng thái sẽ tự cập nhật khi trả phòng.";
            }
            else if (phong.TrangThai == trangThai)
            {
                TempData["Error"] = $"Phòng {phong.SoPhong} đã ở trạng thái này rồi.";
            }
            else
            {
                phong.TrangThai = trangThai;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã chuyển phòng {phong.SoPhong} sang trạng thái \"{DanhSachTrangThai()[trangThai]}\".";
            }

            // Quay lại đúng trang đang xem, giữ nguyên điều kiện tìm/lọc/sắp xếp/phân trang
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (!LaAdmin()) return TuChoiTruyCap();
            if (id == null) return NotFound();

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return NotFound();

            await NapDanhSachChon(phong.MaLoaiPhong);
            return View(phong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhong,SoPhong,MaLoaiPhong,Tang,DonGiaDem,MoTa,TrangThai")] Phong phong)
        {
            if (!LaAdmin()) return TuChoiTruyCap();
            if (id != phong.MaPhong) return NotFound();

            ModelState.Remove(nameof(Phong.LoaiPhong));
            phong.SoPhong = (phong.SoPhong ?? string.Empty).Trim();
            await KiemTraNghiepVu(phong, id);

            if (ModelState.IsValid)
            {
                var hienTai = await _context.Phongs.FindAsync(id);
                if (hienTai == null) return NotFound();

                // Chỉ cập nhật các trường được phép sửa
                hienTai.SoPhong = phong.SoPhong;
                hienTai.MaLoaiPhong = phong.MaLoaiPhong;
                hienTai.Tang = phong.Tang;
                hienTai.DonGiaDem = phong.DonGiaDem;
                hienTai.MoTa = phong.MoTa;
                hienTai.TrangThai = phong.TrangThai;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã cập nhật phòng {hienTai.SoPhong}.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError(nameof(Phong.SoPhong), "Không lưu được. Số phòng có thể đã tồn tại.");
                }
            }

            await NapDanhSachChon(phong.MaLoaiPhong);
            return View(phong);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (!LaAdmin()) return TuChoiTruyCap();
            if (id == null) return NotFound();

            var phong = await _context.Phongs.AsNoTracking()
                .Include(p => p.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhong == id);
            if (phong == null) return NotFound();

            ViewBag.CoLuotDat = await _context.ChiTietDatPhongs.AnyAsync(c => c.MaPhong == id);
            return View(phong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!LaAdmin()) return TuChoiTruyCap();

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null) return RedirectToAction(nameof(Index));

            // Chặn xóa nếu phòng đã có trong phiếu đặt
            if (await _context.ChiTietDatPhongs.AnyAsync(c => c.MaPhong == id))
            {
                TempData["Error"] = $"Không thể xóa phòng {phong.SoPhong} vì đã có trong phiếu đặt phòng.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            try
            {
                _context.Phongs.Remove(phong);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa phòng {phong.SoPhong}.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = $"Không thể xóa phòng {phong.SoPhong}.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }


        // ===== HÀM PHỤ =====
        private IActionResult TuChoiTruyCap()
        {
            // Chưa có trang đăng nhập thì về trang chủ; sau này đổi sang TaiKhoan/DangNhap
            TempData["Error"] = "Bạn cần đăng nhập bằng tài khoản Admin để thực hiện thao tác này.";
            return RedirectToAction("Index", "Home");
        }

        private async Task KiemTraNghiepVu(Phong phong, int? boQuaMaPhong)
        {
            if (!DanhSachTrangThai().ContainsKey(phong.TrangThai ?? string.Empty))
            {
                ModelState.AddModelError(nameof(Phong.TrangThai), "Trạng thái không hợp lệ.");
            }

            var loai = await _context.LoaiPhongs.AsNoTracking()
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == phong.MaLoaiPhong);
            if (loai == null)
            {
                ModelState.AddModelError(nameof(Phong.MaLoaiPhong), "Vui lòng chọn loại phòng hợp lệ.");
            }

            if (!string.IsNullOrEmpty(phong.SoPhong))
            {
                bool trung = await _context.Phongs.AnyAsync(p =>
                    p.SoPhong == phong.SoPhong && (boQuaMaPhong == null || p.MaPhong != boQuaMaPhong));
                if (trung)
                {
                    ModelState.AddModelError(nameof(Phong.SoPhong), "Số phòng này đã tồn tại.");
                }
            }
        }

        // Loại phòng đang hoạt động (giữ cả loại hiện tại khi sửa dù đã ngừng)
        private async Task NapDanhSachChon(int? maDangChon)
        {
            var ds = await _context.LoaiPhongs.AsNoTracking()
                .Where(l => l.TrangThai || l.MaLoaiPhong == maDangChon)
                .OrderBy(l => l.TenLoaiPhong)
                .ToListAsync();
            ViewBag.LoaiPhongList = new SelectList(ds, "MaLoaiPhong", "TenLoaiPhong", maDangChon);
            ViewBag.TrangThaiList = DanhSachTrangThai()
                .Select(kv => new SelectListItem(kv.Value, kv.Key)).ToList();
        }

        private static Dictionary<string, string> DanhSachTrangThai() => new()
        {
            { TrangThaiPhong.SanSang, "Sẵn sàng" },
            { TrangThaiPhong.DangSuDung, "Đang sử dụng" },
            { TrangThaiPhong.TamNgung, "Tạm ngừng" }
        };
    }
}