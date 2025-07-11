namespace DataManagementApi.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}