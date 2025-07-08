using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class CreateInternshipDto
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int PartnerId { get; set; }
        
        [Required]
        public int AcademicYearId { get; set; }
        
        [Required]
        public int SemesterId { get; set; }
        
        public string? ReportUrl { get; set; }
        
        public double? Grade { get; set; }
    }
}