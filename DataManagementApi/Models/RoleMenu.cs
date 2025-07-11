namespace DataManagementApi.Models
{
    // Bảng nối cho quan hệ nhiều-nhiều giữa Role và Menu
    public class RoleMenu
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int MenuId { get; set; }
        public Menu Menu { get; set; } = null!;
    }
} 