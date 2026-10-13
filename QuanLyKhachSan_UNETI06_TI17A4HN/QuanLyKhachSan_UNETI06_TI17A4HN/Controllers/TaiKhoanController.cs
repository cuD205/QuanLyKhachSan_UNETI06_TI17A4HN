// Sinh viên: [Họ và tên] - MSSV: [Mã sinh viên]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Đăng nhập (EF Core + LINQ, kiểm tra mật khẩu, tài khoản khóa), đăng xuất, từ chối truy cập
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Helpers;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            // Đã đăng nhập rồi thì không cần vào lại trang đăng nhập
            if (HttpContext.Session.DaDangNhap())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tenDangNhap = model.TenDangNhap.Trim();

            // Tìm tài khoản theo tên đăng nhập bằng LINQ
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            // So sánh mật khẩu bằng C# (phân biệt chữ hoa/thường); SQL Server mặc định không phân biệt
            // hoa/thường nên không đưa việc so sánh mật khẩu vào câu truy vấn.
            if (taiKhoan == null || !string.Equals(taiKhoan.MatKhau, model.MatKhau, StringComparison.Ordinal))
            {
                // Dùng chung một thông báo để người ngoài không dò được tên đăng nhập nào tồn tại
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Xóa phiên cũ rồi mới ghi thông tin đăng nhập mới
            HttpContext.Session.Clear();
            HttpContext.Session.LuuDangNhap(taiKhoan);

            TempData["ThanhCong"] = $"Xin chào {taiKhoan.HoTen}!";

            // Chỉ chuyển hướng tới đường dẫn nội bộ để tránh open redirect
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất: xóa thông tin đăng nhập khỏi Session rồi về trang chủ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            TempData["ThanhCong"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
