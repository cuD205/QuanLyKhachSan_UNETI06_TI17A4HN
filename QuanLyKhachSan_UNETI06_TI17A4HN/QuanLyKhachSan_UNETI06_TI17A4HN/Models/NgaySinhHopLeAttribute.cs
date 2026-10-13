// Họ và tên: Đinh Ngọc Cường
// Mã sinh viên: 23103100260
// Nội dung thực hiện: Kiểm tra ngày sinh hợp lệ cho khách hàng (Module 3)
using System.ComponentModel.DataAnnotations;

namespace QuanLyKhachSan_UNETI06_TI17A4HN.Models;

// Kiểm tra ngày sinh: không ở tương lai và đủ tuổi tối thiểu
public class NgaySinhHopLeAttribute : ValidationAttribute
{
    public int TuoiToiThieu { get; set; } = 16;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Để [Required] lo trường hợp bỏ trống
        if (value is not DateTime ngaySinh)
        {
            return ValidationResult.Success;
        }

        DateTime homNay = DateTime.Today;

        if (ngaySinh.Date > homNay)
        {
            return new ValidationResult("Ngày sinh không được ở tương lai.");
        }

        // Tính tuổi tròn
        int tuoi = homNay.Year - ngaySinh.Year;
        if (ngaySinh.Date > homNay.AddYears(-tuoi))
        {
            tuoi--;
        }

        if (tuoi < TuoiToiThieu)
        {
            return new ValidationResult($"Khách hàng phải từ {TuoiToiThieu} tuổi trở lên.");
        }

        return ValidationResult.Success;
    }
}