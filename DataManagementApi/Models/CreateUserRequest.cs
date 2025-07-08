using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Keycloak User ID is required")]
        public string KeycloakUserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> RoleIds { get; set; } = new List<int>();
    }
}
