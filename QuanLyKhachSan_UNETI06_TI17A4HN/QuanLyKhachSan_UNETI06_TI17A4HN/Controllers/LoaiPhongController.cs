// Sinh viên: [Nguyen Thanh Liem] - MSSV: [23103100281]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Quản lý loại phòng (danh sách, chi tiết, thêm, sửa, xóa, cập nhật trạng thái) - chỉ Admin
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Filters;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    // Kiểm tra quyền ở Controller: mọi Action bên dưới chỉ Admin mới vào được
    [PhanQuyen(VaiTro.Admin)]
    public class LoaiPhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiPhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoaiPhong?tuKhoa=...&trangThai=1
        public async Task<IActionResult> Index(string? tuKhoa, string? trangThai)
        {
            var query = _context.LoaiPhongs.Include(l => l.Phongs).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(l => l.TenLoaiPhong.Contains(tuKhoa));
            }

            if (trangThai == "1")
            {
                query = query.Where(l => l.TrangThai);
            }
            else if (trangThai == "0")
            {
                query = query.Where(l => !l.TrangThai);
            }

            var danhSach = await query.OrderBy(l => l.MaLoaiPhong).ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            return View(danhSach);
        }

        // GET: LoaiPhong/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == id);

            if (loaiPhong == null)
            {
                return NotFound();
            }
            return View(loaiPhong);
        }

        // GET: LoaiPhong/Create
        public IActionResult Create()
        {
            return View(new LoaiPhong { TrangThai = true });
        }

        // POST: LoaiPhong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenLoaiPhong,SoNguoiToiDa,DonGiaDem,MoTa,TrangThai")] LoaiPhong model)
        {
            model.TenLoaiPhong = model.TenLoaiPhong?.Trim() ?? string.Empty;

            // Quy tắc nghiệp vụ không có sẵn trong Data Annotation: tên loại phòng không được trùng
            if (!string.IsNullOrEmpty(model.TenLoaiPhong) &&
                await _context.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == model.TenLoaiPhong))
            {
                ModelState.AddModelError(nameof(LoaiPhong.TenLoaiPhong), "Tên loại phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.LoaiPhongs.Add(model);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã thêm loại phòng \"{model.TenLoaiPhong}\".";
            return RedirectToAction(nameof(Index));
        }

        // GET: LoaiPhong/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }
            return View(loaiPhong);
        }

        // POST: LoaiPhong/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("MaLoaiPhong,TenLoaiPhong,SoNguoiToiDa,DonGiaDem,MoTa,TrangThai")] LoaiPhong model)
        {
            if (id != model.MaLoaiPhong)
            {
                return NotFound();
            }

            model.TenLoaiPhong = model.TenLoaiPhong?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(model.TenLoaiPhong) &&
                await _context.LoaiPhongs.AnyAsync(l => l.TenLoaiPhong == model.TenLoaiPhong && l.MaLoaiPhong != id))
            {
                ModelState.AddModelError(nameof(LoaiPhong.TenLoaiPhong), "Tên loại phòng đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lấy bản ghi hiện có rồi chỉ cập nhật các trường được phép sửa
            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }

            loaiPhong.TenLoaiPhong = model.TenLoaiPhong;
            loaiPhong.SoNguoiToiDa = model.SoNguoiToiDa;
            loaiPhong.DonGiaDem = model.DonGiaDem;
            loaiPhong.MoTa = model.MoTa;
            loaiPhong.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã cập nhật loại phòng \"{loaiPhong.TenLoaiPhong}\".";
            return RedirectToAction(nameof(Index));
        }

        // GET: LoaiPhong/Delete/5 (trang xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiPhong = await _context.LoaiPhongs
                .Include(l => l.Phongs)
                .FirstOrDefaultAsync(l => l.MaLoaiPhong == id);

            if (loaiPhong == null)
            {
                return NotFound();
            }
            return View(loaiPhong);
        }

        // POST: LoaiPhong/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }

            // Không xóa loại phòng đang có phòng thuộc loại này (tránh dữ liệu phòng mất liên kết)
            if (await _context.Phongs.AnyAsync(p => p.MaLoaiPhong == id))
            {
                TempData["Loi"] = $"Không thể xóa loại phòng \"{loaiPhong.TenLoaiPhong}\" vì đang có phòng thuộc loại này. " +
                                  "Hãy chuyển trạng thái sang ngừng kinh doanh thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.LoaiPhongs.Remove(loaiPhong);
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xóa loại phòng \"{loaiPhong.TenLoaiPhong}\".";
            return RedirectToAction(nameof(Index));
        }

        // POST: LoaiPhong/CapNhatTrangThai/5 (đổi nhanh giữa đang kinh doanh / ngừng kinh doanh)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id)
        {
            var loaiPhong = await _context.LoaiPhongs.FindAsync(id);
            if (loaiPhong == null)
            {
                return NotFound();
            }

            loaiPhong.TrangThai = !loaiPhong.TrangThai;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = loaiPhong.TrangThai
                ? $"Loại phòng \"{loaiPhong.TenLoaiPhong}\" đã chuyển sang đang kinh doanh."
                : $"Loại phòng \"{loaiPhong.TenLoaiPhong}\" đã chuyển sang ngừng kinh doanh.";
            return RedirectToAction(nameof(Index));
        }
    }
}
