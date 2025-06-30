namespace DataManagementApi.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        
        // Foreign key for self-referencing relationship
        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }
        
        public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();
    }
} 