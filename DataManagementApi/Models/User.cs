namespace DataManagementApi.Models
{
    public class User
    {
        public int Id { get; set; }
        
        // Dùng để liên kết với User trong Keycloak, không lưu password ở đây
        public string KeycloakUserId { get; set; } = string.Empty; 

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        
        // Thay cho status, dùng boolean rõ ràng hơn
        public bool IsActive { get; set; } 

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}