// Sinh viên: Dương Minh Đức - MSSV: 23103100282
// Module: Nền tảng dùng chung (Entity / DbContext)
// Nội dung: Khởi tạo Entity Loại phòng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models
{
    public class LoaiPhong
    {
        [Key]
        public int MaLoaiPhong { get; set; }

        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(100)]
        public string TenLoaiPhong { get; set; } = string.Empty;

        [Range(1, 20, ErrorMessage = "Số người tối đa phải lớn hơn 0")]
        public int SoNguoiToiDa { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DonGiaDem { get; set; }

        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public ICollection<Phong> Phongs { get; set; } = new List<Phong>();
    }
}
