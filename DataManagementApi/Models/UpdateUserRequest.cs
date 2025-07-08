using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class UpdateUserRequest
    {
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string? Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        public string? AvatarUrl { get; set; }

        public bool? IsActive { get; set; }

        public List<int>? RoleIds { get; set; }
    }
}
