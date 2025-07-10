namespace DataManagementApi.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ví dụ: "2023-2024"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? DeletedAt { get; set; } // For soft delete
    }
} 