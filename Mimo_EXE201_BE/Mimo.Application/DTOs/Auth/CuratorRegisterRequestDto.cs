using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.Auth
{
    public class CuratorRegisterRequestDto
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(150, ErrorMessage = "Lĩnh vực chuyên môn tối đa 150 ký tự")]
        public string? Specialization { get; set; }
    }
}
