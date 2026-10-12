// Họ và tên: Diện | Mã sinh viên: [MSSV của bạn]
// Module 2: Quản lý và tra cứu Phòng
// Nội dung: Danh sách phòng, hiển thị tên loại phòng
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    public class PhongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhongController(ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}