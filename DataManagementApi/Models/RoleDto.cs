namespace DataManagementApi.Models
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}