namespace DataManagementApi.Models
{
    // Bảng nối cho quan hệ nhiều-nhiều giữa Role và Permission
    public class RolePermission
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
} 