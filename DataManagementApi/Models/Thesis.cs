namespace DataManagementApi.Models
{
    public class Thesis
    {
        public int Id { get; set; }
        public string Title { get; set; }

        // Foreign key to Student
        public int StudentId { get; set; }
        public Student Student { get; set; }

        // Foreign key to AcademicYear
        public int AcademicYearId { get; set; }
        public AcademicYear AcademicYear { get; set; }

        // Foreign key to Semester
        public int SemesterId { get; set; }
        public Semester Semester { get; set; }

        public DateTime SubmissionDate { get; set; }
    }
} 