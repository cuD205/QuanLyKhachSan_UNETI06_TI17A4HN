// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Module 4 - Quản lý phiếu đặt phòng, nhận/trả phòng, Dashboard, thống kê
// Nội dung: Quản lý phiếu đặt phòng - danh sách, tìm theo tên khách, lọc trạng thái, lọc ngày nhận, xem chi tiết (chỉ Admin)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Filters;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    // Kiểm tra quyền ở Controller: mọi Action bên dưới chỉ Admin mới vào được
    [PhanQuyen(VaiTro.Admin)]
    public class PhieuDatPhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhieuDatPhongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PhieuDatPhong?tuKhoa=...&trangThai=...&tuNgay=...&denNgay=...
        public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var query = _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(c => c.Phong)
                .AsQueryable();

            // 1. Tìm theo tên khách hàng
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(p => p.KhachHang!.HoTen.Contains(tuKhoa));
            }

            // 2. Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            // 3. Lọc theo ngày nhận dự kiến (khoảng từ - đến, có thể chỉ nhập một đầu)
            if (tuNgay.HasValue)
            {
                var tu = tuNgay.Value.Date;
                query = query.Where(p => p.NgayNhanDuKien >= tu);
            }
            if (denNgay.HasValue)
            {
                // < ngày kế tiếp để lấy trọn cả ngày "đến"
                var den = denNgay.Value.Date.AddDays(1);
                query = query.Where(p => p.NgayNhanDuKien < den);
            }

            var model = new PhieuDatPhongIndexViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DanhSach = await query.OrderByDescending(p => p.NgayDat).ToListAsync()
            };

            return View(model);
        }

        // GET: PhieuDatPhong/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieu = await _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(c => c.Phong)
                        .ThenInclude(ph => ph!.LoaiPhong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);

            if (phieu == null)
            {
                return NotFound();
            }

            return View(phieu);
        }
    }
}
