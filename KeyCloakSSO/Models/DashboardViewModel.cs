namespace KeyCloakSSO.Models
{
    public class DashboardViewModel
    {
        public string TenNguoiDung { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ThoiGianDangNhap { get; set; }
        public string ChaoMung => $"Chào mừng, {TenNguoiDung}!";
    }
} 