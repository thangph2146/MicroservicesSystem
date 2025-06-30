namespace DataManagementApi.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ví dụ: "users:create", "users:read"
        public string? Description { get; set; }
        public string Module { get; set; } // Ví dụ: "UserManagement", "Academic"

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
} 