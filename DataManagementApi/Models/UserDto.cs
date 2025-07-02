using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string KeycloakUserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
    }

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Keycloak User ID không được để trống")]
        public required string KeycloakUserId { get; set; }

        [Required(ErrorMessage = "Tên người dùng không được để trống")]
        [StringLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public required string Email { get; set; }

        [Url(ErrorMessage = "URL ảnh đại diện không đúng định dạng")]
        public string? AvatarUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserRequest
    {
        [StringLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        public string? Email { get; set; }

        [Url(ErrorMessage = "URL ảnh đại diện không đúng định dạng")]
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
