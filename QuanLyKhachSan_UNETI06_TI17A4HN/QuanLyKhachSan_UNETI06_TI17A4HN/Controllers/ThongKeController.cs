// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Module 4 - Quản lý phiếu đặt phòng, nhận/trả phòng, Dashboard, thống kê
// Nội dung: Dashboard 8 chỉ số tổng quan hệ thống (chỉ Admin)
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
    }
}
