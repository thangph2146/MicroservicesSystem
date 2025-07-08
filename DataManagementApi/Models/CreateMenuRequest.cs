namespace DataManagementApi.Models
{
    public class CreateMenuRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentId { get; set; }
    }
}
