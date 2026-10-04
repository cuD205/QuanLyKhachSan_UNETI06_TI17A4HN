// Họ và tên: Dương Minh Đức
// Mã sinh viên: 23103100282
// Nội dung thực hiện: Seed du lieu mau cho 6 bang (LoaiPhong, Phong, TaiKhoan, KhachHang, PhieuDatPhong, ChiTietDatPhong)
using QuanLyKhachSan_UNETI06_TI17A4HN.Models;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Data
{
    public static class DbInitializer
    {
        // Goi ham nay 1 lan trong Program.cs sau khi app.Build(), truoc app.Run()
        public static void Seed(ApplicationDbContext context)
        {
            // Idempotent: neu da co du lieu (vi du da co LoaiPhong) thi khong seed lai,
            // tranh chen trung moi lan chay app.
            if (context.LoaiPhongs.Any())
            {
                return;
            }

            // ===== 1. LoaiPhong (5 loai, dung yeu cau toi thieu muc 15) =====
            var loaiPhongs = new List<LoaiPhong>
            {
                new LoaiPhong { TenLoaiPhong = "Phong Don",      SoNguoiToiDa = 1, DonGiaDem = 350000,  MoTa = "Phong don gian, phu hop 1 nguoi.",             TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Phong Doi",      SoNguoiToiDa = 2, DonGiaDem = 550000,  MoTa = "Phong tieu chuan cho 2 nguoi.",                TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Superior",       SoNguoiToiDa = 2, DonGiaDem = 750000,  MoTa = "Phong cao cap hon, view dep.",                 TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Deluxe",         SoNguoiToiDa = 3, DonGiaDem = 1200000, MoTa = "Phong rong rai, day du tien nghi.",            TrangThai = true },
                new LoaiPhong { TenLoaiPhong = "Suite Gia Dinh", SoNguoiToiDa = 5, DonGiaDem = 2200000, MoTa = "Phong suite danh cho gia dinh/nhom dong.",     TrangThai = true },
            };
            context.LoaiPhongs.AddRange(loaiPhongs);
            context.SaveChanges(); // can SaveChanges truoc de co MaLoaiPhong that cho Phong tham chieu

            // ===== 2. Phong (20 phong, du muc toi thieu, trai deu 5 loai + 5 tang) =====
            var phongs = new List<Phong>();
            var trangThaiXoayVong = new[] { TrangThaiPhong.SanSang, TrangThaiPhong.SanSang, TrangThaiPhong.SanSang, TrangThaiPhong.DangSuDung, TrangThaiPhong.TamNgung };
            int maPhongCounter = 0;
            for (int tang = 1; tang <= 5; tang++)
            {
                for (int i = 1; i <= 4; i++)
                {
                    var loai = loaiPhongs[(tang - 1) % loaiPhongs.Count];
                    string soPhong = $"{tang}0{i}"; // vi du 101, 102, 201, 202...
                    phongs.Add(new Phong
                    {
                        SoPhong = soPhong,
                        MaLoaiPhong = loai.MaLoaiPhong,
                        Tang = tang,
                        DonGiaDem = loai.DonGiaDem, // mac dinh bang gia loai phong, co the chenh neu muon
                        MoTa = $"Phong {soPhong} thuoc loai {loai.TenLoaiPhong}.",
                        TrangThai = trangThaiXoayVong[maPhongCounter % trangThaiXoayVong.Length]
                    });
                    maPhongCounter++;
                }
            }
            context.Phongs.AddRange(phongs);
            context.SaveChanges();

            // ===== 3. TaiKhoan (2 Admin + 15 Khach hang, mat khau dang chu thuong - CHUA HASH) =====
            var taiKhoanAdmins = new List<TaiKhoan>
            {
                new TaiKhoan { TenDangNhap = "admin1", MatKhau = "admin123", HoTen = "Quan Tri Vien 1", Email = "admin1@khachsan.vn", VaiTro = VaiTro.Admin, TrangThai = true },
                new TaiKhoan { TenDangNhap = "admin2", MatKhau = "admin123", HoTen = "Quan Tri Vien 2", Email = "admin2@khachsan.vn", VaiTro = VaiTro.Admin, TrangThai = true },
            };
            context.TaiKhoans.AddRange(taiKhoanAdmins);

            string[] hoTenKhach = {
                "Nguyen Van An","Tran Thi Binh","Le Van Cuong","Pham Thi Dung","Hoang Van Em",
                "Vu Thi Phuong","Dang Van Giang","Bui Thi Hoa","Do Van Inh","Ngo Thi Kim",
                "Duong Van Long","Ly Thi Mai","Phan Van Nam","Ta Thi Oanh","Vo Van Phuc"
            };
            var taiKhoanKhachs = new List<TaiKhoan>();
            for (int i = 0; i < hoTenKhach.Length; i++)
            {
                int stt = i + 1;
                taiKhoanKhachs.Add(new TaiKhoan
                {
                    TenDangNhap = $"khach{stt:D2}",
                    MatKhau = "123456",
                    HoTen = hoTenKhach[i],
                    Email = $"khach{stt:D2}@gmail.com",
                    VaiTro = VaiTro.KhachHang,
                    TrangThai = true
                });
            }
            context.TaiKhoans.AddRange(taiKhoanKhachs);
            context.SaveChanges(); // can SaveChanges truoc de co MaTaiKhoan that cho KhachHang tham chieu

            // ===== 4. KhachHang (15, moi nguoi gan 1-1 voi TaiKhoan KhachHang vua tao) =====
            var khachHangs = new List<KhachHang>();
            string[] gioiTinhXoayVong = { "Nam", "Nu" };
            var rand = new Random(2026); // seed co dinh de du lieu on dinh moi lan chay lai tu dau
            for (int i = 0; i < taiKhoanKhachs.Count; i++)
            {
                int stt = i + 1;
                khachHangs.Add(new KhachHang
                {
                    MaTaiKhoan = taiKhoanKhachs[i].MaTaiKhoan,
                    HoTen = taiKhoanKhachs[i].HoTen,
                    NgaySinh = new DateTime(1985 + (i % 20), 1 + (i % 12), 1 + (i % 28)),
                    GioiTinh = gioiTinhXoayVong[i % 2],
                    SoDienThoai = $"09{(10000000 + i * 137):D8}",
                    Email = taiKhoanKhachs[i].Email,
                    CCCD = $"0{(79010000000 + i):D11}".Substring(0, 12),
                    DiaChi = $"So {stt}, Duong Le Loi, Quan {1 + (i % 10)}, TP. Ha Noi",
                    NgayDangKy = DateTime.Now.AddDays(-rand.Next(30, 400)),
                    TrangThai = true
                });
            }
            context.KhachHangs.AddRange(khachHangs);
            context.SaveChanges();

            // ===== 5 & 6. PhieuDatPhong + ChiTietDatPhong (18 phieu, trai deu 5 trang thai) =====
            // Ke hoach trang thai: 4 ChoXacNhan, 4 DaXacNhan, 4 DaNhanPhong, 4 DaTraPhong, 2 DaHuy = 18 phieu
            var ketHoachTrangThai = new List<string>();
            ketHoachTrangThai.AddRange(Enumerable.Repeat(TrangThaiPhieu.ChoXacNhan, 4));
            ketHoachTrangThai.AddRange(Enumerable.Repeat(TrangThaiPhieu.DaXacNhan, 4));
            ketHoachTrangThai.AddRange(Enumerable.Repeat(TrangThaiPhieu.DaNhanPhong, 4));
            ketHoachTrangThai.AddRange(Enumerable.Repeat(TrangThaiPhieu.DaTraPhong, 4));
            ketHoachTrangThai.AddRange(Enumerable.Repeat(TrangThaiPhieu.DaHuy, 2));

            for (int i = 0; i < ketHoachTrangThai.Count; i++)
            {
                string trangThai = ketHoachTrangThai[i];
                var khachHang = khachHangs[i % khachHangs.Count];

                // Chon ngay nhan/tra theo dung logic cua tung trang thai:
                // - ChoXacNhan / DaXacNhan: ngay nhan trong TUONG LAI (chua den luu tru)
                // - DaNhanPhong: ngay nhan da qua, ngay tra con trong TUONG LAI (dang o)
                // - DaTraPhong / DaHuy: toan bo trong QUA KHU (da xong hoac da huy)
                DateTime ngayNhan, ngayTra, ngayDat;
                if (trangThai == TrangThaiPhieu.ChoXacNhan || trangThai == TrangThaiPhieu.DaXacNhan)
                {
                    ngayDat = DateTime.Now.AddDays(-rand.Next(1, 5));
                    ngayNhan = DateTime.Now.AddDays(rand.Next(2, 20));
                    ngayTra = ngayNhan.AddDays(rand.Next(1, 4));
                }
                else if (trangThai == TrangThaiPhieu.DaNhanPhong)
                {
                    ngayNhan = DateTime.Now.AddDays(-rand.Next(1, 3));
                    ngayTra = DateTime.Now.AddDays(rand.Next(1, 4));
                    ngayDat = ngayNhan.AddDays(-rand.Next(2, 10));
                }
                else // DaTraPhong, DaHuy
                {
                    ngayNhan = DateTime.Now.AddDays(-rand.Next(10, 60));
                    ngayTra = ngayNhan.AddDays(rand.Next(1, 5));
                    ngayDat = ngayNhan.AddDays(-rand.Next(2, 10));
                }

                int soDem = Math.Max(1, (ngayTra.Date - ngayNhan.Date).Days);

                // Moi phieu dat 1 phong rieng (dung chi so i de trai deu, tranh trung phong cung thoi gian)
                var phong = phongs[i % phongs.Count];
                decimal thanhTien = soDem * phong.DonGiaDem;

                var phieu = new PhieuDatPhong
                {
                    MaKhachHang = khachHang.MaKhachHang,
                    NgayDat = ngayDat,
                    NgayNhanDuKien = ngayNhan,
                    NgayTraDuKien = ngayTra,
                    TongTienDuKien = thanhTien, // phieu chi co 1 phong nen tong tien = thanh tien dong duy nhat
                    TrangThai = trangThai
                };
                context.PhieuDatPhongs.Add(phieu);
                context.SaveChanges(); // can MaPhieuDat that truoc khi tao ChiTietDatPhong

                context.ChiTietDatPhongs.Add(new ChiTietDatPhong
                {
                    MaPhieuDat = phieu.MaPhieuDat,
                    MaPhong = phong.MaPhong,
                    DonGiaDem = phong.DonGiaDem,
                    SoDem = soDem,
                    ThanhTien = thanhTien
                });
            }
            context.SaveChanges();
        }
    }
}
