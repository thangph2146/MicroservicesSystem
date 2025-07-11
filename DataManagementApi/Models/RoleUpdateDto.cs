namespace DataManagementApi.Models
{
    public class RoleUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<int>? PermissionIds { get; set; }
    }
} 