namespace DataManagementApi.Models
{
    public class Semester
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ví dụ: "Học kỳ 1"
        public int AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
} 