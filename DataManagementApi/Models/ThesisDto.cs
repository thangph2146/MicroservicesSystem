namespace DataManagementApi.Models
{
    public class ThesisDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        // Student information
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }

        // Supervisor information
        public int SupervisorId { get; set; }
        public string? SupervisorName { get; set; }
        public string? SupervisorEmail { get; set; }

        // Examiner information (optional)
        public int? ExaminerId { get; set; }
        public string? ExaminerName { get; set; }
        public string? ExaminerEmail { get; set; }

        // Academic information
        public int AcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }

        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }

        public DateTime SubmissionDate { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
