using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class AcademicYearUpdateDto
    {
        [Required(ErrorMessage = "Tên niên khóa không được để trống")]
        [StringLength(100, ErrorMessage = "Tên niên khóa không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }
    }
} 