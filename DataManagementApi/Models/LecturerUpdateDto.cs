using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class LecturerUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public int? DepartmentId { get; set; }
        
        [StringLength(50)]
        public string? AcademicRank { get; set; }
        
        [StringLength(50)]
        public string? Degree { get; set; }
        
        [StringLength(200)]
        public string? Specialization { get; set; }
        
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 