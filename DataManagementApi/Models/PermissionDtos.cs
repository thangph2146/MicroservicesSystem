namespace DataManagementApi.Models
{
    public class CreatePermissionData
    {
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdatePermissionData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 