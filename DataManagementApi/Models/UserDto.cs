using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string KeycloakUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
    }

}
