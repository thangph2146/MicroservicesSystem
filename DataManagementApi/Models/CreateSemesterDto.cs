using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class CreateSemesterDto
    {
        [Required(ErrorMessage = "Tên học kỳ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên học kỳ không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã năm học là bắt buộc")]
        public int AcademicYearId { get; set; }
    }
}
