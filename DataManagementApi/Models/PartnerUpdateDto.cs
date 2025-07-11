using System.ComponentModel.DataAnnotations;

namespace DataManagementApi.Models
{
    public class PartnerUpdateDto
    {
        [Required(ErrorMessage = "Tên đối tác không được để trống.")]
        [StringLength(200, ErrorMessage = "Tên đối tác không được vượt quá 200 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string? Address { get; set; }

        [StringLength(200, ErrorMessage = "Website không được vượt quá 200 ký tự.")]
        public string? Website { get; set; }
        
        public string? Description { get; set; }
        
        [StringLength(100, ErrorMessage = "Tên người liên hệ không được vượt quá 100 ký tự.")]
        public string? ContactPerson { get; set; }
        
        public bool IsActive { get; set; }
    }
} 