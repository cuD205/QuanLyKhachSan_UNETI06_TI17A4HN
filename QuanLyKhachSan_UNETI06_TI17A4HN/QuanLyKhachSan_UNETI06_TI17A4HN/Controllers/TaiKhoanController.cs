// Sinh viên: [Nguyen Thanh Liem] - MSSV: [23103100281]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Đăng nhập (EF Core + LINQ, kiểm tra mật khẩu, tài khoản khóa), đăng xuất, từ chối truy cập
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyKhachSan_UNETI06_TI17A4HN.Data;
using QuanLyKhachSan_UNETI06_TI17A4HN.Helpers;
using QuanLyKhachSan_UNETI06_TI17A4HN.ViewModels;
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

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


        [HttpGet]
        public IActionResult DangKy()
        {
            if (HttpContext.Session.DaDangNhap())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new DangKyViewModel());
        }

        // Đăng ký: tạo TaiKhoan (vai trò KhachHang) và KhachHang tương ứng (quan hệ 1 - 1) trong cùng một lần lưu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            model.TenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            model.HoTen = model.HoTen?.Trim() ?? string.Empty;
            model.CCCD = model.CCCD?.Trim() ?? string.Empty;

            if (model.NgaySinh.HasValue && (model.NgaySinh.Value.Date > DateTime.Today || model.NgaySinh.Value.Year < 1900))
            {
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không hợp lệ.");
            }

            // Các quy tắc không có sẵn trong Data Annotation: tên đăng nhập và CCCD không được trùng
            if (!string.IsNullOrEmpty(model.TenDangNhap) &&
                await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            }

            if (!string.IsNullOrEmpty(model.CCCD) &&
                await _context.KhachHangs.AnyAsync(k => k.CCCD == model.CCCD))
            {
                ModelState.AddModelError(nameof(model.CCCD), "CCCD này đã được đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = model.MatKhau,
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = VaiTro.KhachHang,   // tự đăng ký chỉ được làm khách hàng, không thể tự chọn Admin
                TrangThai = true,
                KhachHang = new KhachHang
                {
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh!.Value,
                    GioiTinh = model.GioiTinh,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    CCCD = model.CCCD,
                    DiaChi = model.DiaChi,
                    NgayDangKy = DateTime.Now,
                    TrangThai = true
                }
            };

            try
            {
                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Không thể đăng ký: tên đăng nhập hoặc CCCD đã tồn tại.");
                return View(model);
            }

            TempData["ThanhCong"] = "Đăng ký thành công. Mời bạn đăng nhập.";
            return RedirectToAction(nameof(DangNhap));
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
