namespace DataManagementApi.Models
{
    public class InternshipUpdateDto
    {
        public int? StudentId { get; set; }
        public int? PartnerId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? SemesterId { get; set; }
        public double? Grade { get; set; }
        public string? ReportUrl { get; set; }
    }
} 