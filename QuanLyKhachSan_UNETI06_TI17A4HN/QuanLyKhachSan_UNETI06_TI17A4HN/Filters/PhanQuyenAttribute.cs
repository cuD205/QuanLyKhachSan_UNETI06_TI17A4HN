// Sinh viên: [Nguyen Thanh Liem] - MSSV: [23103100281]
// Module: Module 1 - Tài khoản, đăng nhập, phân quyền, quản lý loại phòng
// Nội dung: Phân quyền ở phía Controller bằng Authorization Filter dựa trên Session - cả nhóm dùng chung
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyKhachSan_UNETI06_TI17A4HN.Helpers;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Filters
{
    // Cách dùng (đặt trên Controller để áp dụng cho mọi Action, hoặc trên từng Action):
    //   [PhanQuyen(VaiTro.Admin)]                    -> chỉ Admin
    //   [PhanQuyen(VaiTro.KhachHang)]                -> chỉ Khách hàng
    //   [PhanQuyen(VaiTro.Admin, VaiTro.KhachHang)]  -> cả hai vai trò
    //   [PhanQuyen]                                  -> chỉ cần đã đăng nhập
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class PhanQuyenAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _vaiTroChoPhep;

        public PhanQuyenAttribute(params string[] vaiTroChoPhep)
        {
            _vaiTroChoPhep = vaiTroChoPhep;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var session = context.HttpContext.Session;

            // 1. Chưa đăng nhập -> chuyển về trang đăng nhập (chỉ nhớ returnUrl với request GET)
            if (!session.DaDangNhap())
            {
                string? returnUrl = HttpMethods.IsGet(request.Method)
                    ? $"{request.Path}{request.QueryString}"
                    : null;
                context.Result = new RedirectToActionResult("DangNhap", "TaiKhoan", new { returnUrl });
                return;
            }

            // 2. Đã đăng nhập nhưng sai vai trò -> trang từ chối truy cập (HTTP 403)
            if (_vaiTroChoPhep.Length > 0)
            {
                var vaiTro = session.LayVaiTro();
                if (vaiTro == null || !_vaiTroChoPhep.Contains(vaiTro))
                {
                    context.Result = new RedirectToActionResult("TuChoiTruyCap", "TaiKhoan", null);
                }
            }
        }
    }
}
