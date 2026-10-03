// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Phòng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class Phong
    {
        [Key]
        public int MaPhong { get; set; }

        [Required(ErrorMessage = "Số phòng không được để trống")]
        [StringLength(10)]
        public string SoPhong { get; set; } = string.Empty;

        public int MaLoaiPhong { get; set; }

        [Range(1, 100, ErrorMessage = "Tầng phải lớn hơn 0")]
        public int Tang { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaDem { get; set; }

        public string? MoTa { get; set; }

        [Required, StringLength(20)]
        public string TrangThai { get; set; } = TrangThaiPhong.SanSang;

        public LoaiPhong? LoaiPhong { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = new List<ChiTietDatPhong>();
    }
}
