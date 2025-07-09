namespace DataManagementApi.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }

        // Cấu trúc cha-con cho menu
        public int? ParentId { get; set; }
        public Menu? Parent { get; set; }
        public ICollection<Menu> ChildMenus { get; set; } = new List<Menu>();
        
        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();

        public DateTime? DeletedAt { get; set; }
    }
}