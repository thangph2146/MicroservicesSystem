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
        public required string KeycloakUserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public bool? IsActive { get; set; }
        public List<int>? RoleIds { get; set; }
    }
}
