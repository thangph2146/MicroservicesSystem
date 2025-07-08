using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DataManagementApi.Models
{
    public class CreateSemesterDto
    {
        [Required(ErrorMessage = "Tên học kỳ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên học kỳ không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã năm học là bắt buộc")]
        public int AcademicYearId { get; set; }

        // Required AcademicYear property with proper initialization
        [Required(ErrorMessage = "Thông tin năm học là bắt buộc")]
        public AcademicYear AcademicYear { get; set; } = new AcademicYear();

        // Constructor to ensure AcademicYear is properly initialized
        public CreateSemesterDto()
        {
            // No additional initialization needed due to property initializer above
        }

        // Constructor for when values are provided
        public CreateSemesterDto(string name, int academicYearId)
        {
            Name = name;
            AcademicYearId = academicYearId;
            AcademicYear = new AcademicYear { Id = academicYearId };
        }
    }
}
