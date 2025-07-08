using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class CreateThesisDto
    {
        [Required(ErrorMessage = "Tiêu đề khóa luận là bắt buộc")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ID sinh viên là bắt buộc")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "ID năm học là bắt buộc")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "ID học kỳ là bắt buộc")]
        public int SemesterId { get; set; }

        [Required(ErrorMessage = "Ngày nộp là bắt buộc")]
        public DateTime SubmissionDate { get; set; }
    }
}
