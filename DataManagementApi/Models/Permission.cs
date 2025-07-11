using System.Text.Json.Serialization;

namespace DataManagementApi.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"
        public string Module { get; set; } = string.Empty; // e.g., "Users", "Roles", "Products"
        public string? Description { get; set; }
        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
} 