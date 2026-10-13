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
        public async Task<IActionResult> Index()
        {
            // Include để lấy kèm Tên loại phòng; sắp xếp số phòng theo thứ tự số
            var phongs = await _context.Phongs
                .AsNoTracking()
                .Include(p => p.LoaiPhong)
                .OrderBy(p => p.SoPhong.Length).ThenBy(p => p.SoPhong)
                .ToListAsync();

            var vm = new PhongIndexViewModel { Phongs = phongs };
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