namespace DataManagementApi.Models
{
    public class Internship
    {
        public int Id { get; set; }

        // Foreign key to User (not Student)
        public int StudentId { get; set; }
        public User? Student { get; set; }

        // Foreign key to Partner
        public int PartnerId { get; set; }
        public Partner? Partner { get; set; }

        // Foreign key to AcademicYear
        public int AcademicYearId { get; set; }
        public AcademicYear? AcademicYear { get; set; }

        // Foreign key to Semester
        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }

        public string? ReportUrl { get; set; }
        public double? Grade { get; set; }
    }
}