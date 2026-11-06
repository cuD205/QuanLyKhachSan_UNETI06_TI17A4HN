// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: Quản lý phiếu đặt phòng - danh sách, tìm/lọc, chi tiết, xác nhận, hủy, nhận phòng, trả phòng, kiểm tra trùng lịch, xuất Excel (chỉ Admin)
using ClosedXML.Excel;
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

        // ============================ BƯỚC 1: XEM / TÌM / LỌC ============================

        // GET: PhieuDatPhong?tuKhoa=...&trangThai=...&tuNgay=...&denNgay=...
        public async Task<IActionResult> Index(string? tuKhoa, string? trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var model = new PhieuDatPhongIndexViewModel
            {
                TuKhoa = string.IsNullOrWhiteSpace(tuKhoa) ? tuKhoa : tuKhoa.Trim(),
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                DanhSach = await TaoTruyVanLoc(tuKhoa, trangThai, tuNgay, denNgay)
                    .OrderByDescending(p => p.NgayDat)
                    .ToListAsync()
            };

            return View(model);
        }

        // GET: PhieuDatPhong/XuatExcel?tuKhoa=...&trangThai=...&tuNgay=...&denNgay=...
        // Xuất đúng danh sách đang hiển thị (cùng điều kiện lọc với Index) ra file Excel .xlsx
        public async Task<IActionResult> XuatExcel(string? tuKhoa, string? trangThai, DateTime? tuNgay, DateTime? denNgay)
        {
            var danhSach = await TaoTruyVanLoc(tuKhoa, trangThai, tuNgay, denNgay)
                .OrderByDescending(p => p.NgayDat)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Phiếu đặt phòng");

            // Dòng tiêu đề
            string[] tieuDe = { "Mã phiếu", "Khách hàng", "Phòng", "Ngày đặt", "Ngày nhận dự kiến",
                                "Ngày trả dự kiến", "Tổng tiền dự kiến (đ)", "Trạng thái" };
            for (int i = 0; i < tieuDe.Length; i++)
            {
                sheet.Cell(1, i + 1).Value = tieuDe[i];
            }
            var dongTieuDe = sheet.Range(1, 1, 1, tieuDe.Length);
            dongTieuDe.Style.Font.Bold = true;
            dongTieuDe.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F3D3E");
            dongTieuDe.Style.Font.FontColor = XLColor.White;

            // Mỗi phiếu một dòng
            int dong = 2;
            foreach (var p in danhSach)
            {
                sheet.Cell(dong, 1).Value = p.MaPhieuDat;
                sheet.Cell(dong, 2).Value = p.KhachHang?.HoTen ?? "";
                sheet.Cell(dong, 3).Value = string.Join(", ", p.ChiTietDatPhongs.Select(c => c.Phong?.SoPhong));
                sheet.Cell(dong, 4).Value = p.NgayDat;
                sheet.Cell(dong, 5).Value = p.NgayNhanDuKien;
                sheet.Cell(dong, 6).Value = p.NgayTraDuKien;
                sheet.Cell(dong, 7).Value = p.TongTienDuKien;
                sheet.Cell(dong, 8).Value = TenTrangThaiPhieu(p.TrangThai);
                dong++;
            }

            // Định dạng ngày và tiền, tự giãn độ rộng cột
            sheet.Column(4).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            sheet.Column(5).Style.DateFormat.Format = "dd/MM/yyyy";
            sheet.Column(6).Style.DateFormat.Format = "dd/MM/yyyy";
            sheet.Column(7).Style.NumberFormat.Format = "#,##0";
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            string tenFile = $"PhieuDatPhong_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                tenFile);
        }

        // Dùng chung cho Index và XuatExcel để hai nơi luôn lọc giống hệt nhau
        private IQueryable<PhieuDatPhong> TaoTruyVanLoc(string? tuKhoa, string? trangThai, DateTime? tuNgay, DateTime? denNgay)
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

            return query;
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

        // ============================ BƯỚC 2: CHUYỂN TRẠNG THÁI ============================
        // Luồng: ChoXacNhan -> DaXacNhan -> DaNhanPhong -> DaTraPhong (hoặc DaHuy khi chưa nhận phòng)
        // Mọi Action bên dưới là POST + AntiForgeryToken vì làm thay đổi dữ liệu.

        // POST: PhieuDatPhong/XacNhan/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            var phieu = await LayPhieuAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.ChoXacNhan)
                return TuChoi(id, "Chỉ xác nhận được phiếu đang ở trạng thái Chờ xác nhận.");

            // Ngày trả phải sau ngày nhận (phiếu sai ngày không được xác nhận)
            if (phieu.NgayTraDuKien.Date <= phieu.NgayNhanDuKien.Date)
                return TuChoi(id, "Ngày trả phải sau ngày nhận nên không thể xác nhận phiếu này.");

            // Không xác nhận phòng đã ngừng phục vụ
            foreach (var ct in phieu.ChiTietDatPhongs)
            {
                if (ct.Phong != null && ct.Phong.TrangThai == TrangThaiPhong.TamNgung)
                    return TuChoi(id, $"Phòng {ct.Phong.SoPhong} đang tạm ngừng phục vụ nên không thể xác nhận phiếu.");
            }

            // Không để một phòng có hai lượt lưu trú đã xác nhận trùng khoảng thời gian
            string? loiTrung = await KiemTraTrungLichAsync(phieu);
            if (loiTrung != null) return TuChoi(id, loiTrung);

            phieu.TrangThai = TrangThaiPhieu.DaXacNhan;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xác nhận phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PhieuDatPhong/Huy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var phieu = await LayPhieuAsync(id);
            if (phieu == null) return NotFound();

            // Chỉ hủy được khi khách chưa nhận phòng
            if (phieu.TrangThai != TrangThaiPhieu.ChoXacNhan && phieu.TrangThai != TrangThaiPhieu.DaXacNhan)
                return TuChoi(id, "Chỉ hủy được phiếu đang Chờ xác nhận hoặc Đã xác nhận (chưa nhận phòng).");

            phieu.TrangThai = TrangThaiPhieu.DaHuy;
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã hủy phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PhieuDatPhong/NhanPhong/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhanPhong(int id)
        {
            var phieu = await LayPhieuAsync(id);
            if (phieu == null) return NotFound();

            // 1. Phiếu phải đã được xác nhận
            if (phieu.TrangThai != TrangThaiPhieu.DaXacNhan)
                return TuChoi(id, "Chỉ cho nhận phòng khi phiếu đã được xác nhận.");

            // 2. Mọi phòng trong phiếu phải sẵn sàng sử dụng
            foreach (var ct in phieu.ChiTietDatPhongs)
            {
                if (ct.Phong == null || ct.Phong.TrangThai != TrangThaiPhong.SanSang)
                {
                    string soPhong = ct.Phong?.SoPhong ?? ct.MaPhong.ToString();
                    string tt = TenTrangThaiPhong(ct.Phong?.TrangThai);
                    return TuChoi(id, $"Phòng {soPhong} đang ở trạng thái \"{tt}\" nên chưa thể nhận phòng.");
                }
            }

            // 3. Cập nhật đồng thời phiếu và phòng (một SaveChanges = một giao dịch)
            phieu.TrangThai = TrangThaiPhieu.DaNhanPhong;
            foreach (var ct in phieu.ChiTietDatPhongs)
            {
                ct.Phong!.TrangThai = TrangThaiPhong.DangSuDung;
            }
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xác nhận nhận phòng cho phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: PhieuDatPhong/TraPhong/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraPhong(int id)
        {
            var phieu = await LayPhieuAsync(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.DaNhanPhong)
                return TuChoi(id, "Chỉ cho trả phòng khi phiếu đang ở trạng thái Đã nhận phòng.");

            // Cập nhật phiếu và trả phòng về trạng thái sẵn sàng
            phieu.TrangThai = TrangThaiPhieu.DaTraPhong;
            foreach (var ct in phieu.ChiTietDatPhongs)
            {
                if (ct.Phong != null && ct.Phong.TrangThai == TrangThaiPhong.DangSuDung)
                {
                    ct.Phong.TrangThai = TrangThaiPhong.SanSang;
                }
            }
            await _context.SaveChangesAsync();

            TempData["ThanhCong"] = $"Đã xác nhận trả phòng cho phiếu #{id}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ============================ HÀM DÙNG CHUNG ============================

        // Nạp phiếu kèm khách hàng và các phòng (có theo dõi thay đổi để SaveChanges ghi được)
        private Task<PhieuDatPhong?> LayPhieuAsync(int id)
        {
            return _context.PhieuDatPhongs
                .Include(p => p.KhachHang)
                .Include(p => p.ChiTietDatPhongs)
                    .ThenInclude(c => c.Phong)
                .FirstOrDefaultAsync(p => p.MaPhieuDat == id);
        }

        // Báo lỗi nghiệp vụ lên đầu trang chi tiết (layout hiển thị TempData["Loi"])
        private IActionResult TuChoi(int id, string thongBao)
        {
            TempData["Loi"] = thongBao;
            return RedirectToAction(nameof(Details), new { id });
        }

        // Kiểm tra từng phòng của phiếu có trùng khoảng ngày với phiếu KHÁC đã xác nhận / đang ở không.
        // Hai khoảng [nhan, tra) trùng nhau khi: nhanCu < traMoi VÀ traCu > nhanMoi.
        // So sánh theo NGÀY (.Date) nên ngày trả của người này có thể là ngày nhận của người kia.
        // Trả về null nếu không trùng, ngược lại trả về nội dung lỗi.
        private async Task<string?> KiemTraTrungLichAsync(PhieuDatPhong phieu)
        {
            DateTime nhan = phieu.NgayNhanDuKien.Date;
            DateTime tra = phieu.NgayTraDuKien.Date;
            int maPhieu = phieu.MaPhieuDat;

            foreach (var ct in phieu.ChiTietDatPhongs)
            {
                int maPhong = ct.MaPhong;

                bool trung = await _context.ChiTietDatPhongs.AnyAsync(c =>
                    c.MaPhong == maPhong
                    && c.MaPhieuDat != maPhieu
                    && (c.PhieuDatPhong!.TrangThai == TrangThaiPhieu.DaXacNhan
                        || c.PhieuDatPhong.TrangThai == TrangThaiPhieu.DaNhanPhong)
                    && c.PhieuDatPhong.NgayNhanDuKien.Date < tra
                    && c.PhieuDatPhong.NgayTraDuKien.Date > nhan);

                if (trung)
                {
                    string soPhong = ct.Phong?.SoPhong ?? maPhong.ToString();
                    return $"Phòng {soPhong} đã có phiếu khác được xác nhận trùng khoảng thời gian " +
                           $"{nhan:dd/MM/yyyy} - {tra:dd/MM/yyyy}.";
                }
            }
            return null;
        }

        private static string TenTrangThaiPhieu(string trangThai) => trangThai switch
        {
            TrangThaiPhieu.ChoXacNhan => "Chờ xác nhận",
            TrangThaiPhieu.DaXacNhan => "Đã xác nhận",
            TrangThaiPhieu.DaNhanPhong => "Đã nhận phòng",
            TrangThaiPhieu.DaTraPhong => "Đã trả phòng",
            TrangThaiPhieu.DaHuy => "Đã hủy",
            _ => trangThai
        };

        private static string TenTrangThaiPhong(string? trangThai) => trangThai switch
        {
            TrangThaiPhong.SanSang => "Sẵn sàng",
            TrangThaiPhong.DangSuDung => "Đang sử dụng",
            TrangThaiPhong.TamNgung => "Tạm ngừng",
            _ => "Không xác định"
        };
    }
}
