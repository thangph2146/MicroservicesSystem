namespace DataManagementApi.Models
{
    public class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
    }
}
