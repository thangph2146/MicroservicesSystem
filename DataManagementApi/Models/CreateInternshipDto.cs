using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class CreateInternshipDto
    {
        [Required(ErrorMessage = "Mã sinh viên là bắt buộc")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Mã đối tác là bắt buộc")]
        public int PartnerId { get; set; }

        [Required(ErrorMessage = "Mã năm học là bắt buộc")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Mã học kỳ là bắt buộc")]
        public int SemesterId { get; set; }

        public string? ReportUrl { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public double? Grade { get; set; }
    }
}