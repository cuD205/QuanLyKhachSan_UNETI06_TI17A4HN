// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Kiểm tra đăng nhập và vai trò ở Controller (phân quyền Module 3)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Helpers
{
    // Dùng: [YeuCauVaiTro(VaiTro.Admin)] hoặc [YeuCauVaiTro(VaiTro.KhachHang)]
    public class YeuCauVaiTroAttribute : ActionFilterAttribute
    {
        private readonly string[] _vaiTros;

        public YeuCauVaiTroAttribute(params string[] vaiTros)
        {
            _vaiTros = vaiTros;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var maTaiKhoan = session.LayMaTaiKhoan();
            var vaiTro = session.LayVaiTro();

            // Chưa đăng nhập -> về trang đăng nhập
            if (maTaiKhoan == null || vaiTro == null)
            {
                context.Result = new RedirectResult(SessionKeys.DuongDanDangNhap);
                return;
            }

            // Đã đăng nhập nhưng sai vai trò -> về trang chủ kèm thông báo
            if (_vaiTros.Length > 0 && !_vaiTros.Contains(vaiTro))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                }
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}