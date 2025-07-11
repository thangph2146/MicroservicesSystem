namespace DataManagementApi.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        
        // Foreign key for self-referencing relationship
        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }
        
        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

        public DateTime? DeletedAt { get; set; }
    }
}