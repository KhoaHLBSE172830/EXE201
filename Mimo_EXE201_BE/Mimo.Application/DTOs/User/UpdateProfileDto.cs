using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? AvatarUrl { get; set; }

        [MaxLength(50)]
        public string? Timezone { get; set; }
    }
}

