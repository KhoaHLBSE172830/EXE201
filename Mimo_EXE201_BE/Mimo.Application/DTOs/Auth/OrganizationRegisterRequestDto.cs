using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.Auth
{
    public class OrganizationRegisterRequestDto
    {
        [Required(ErrorMessage = "Tên trường học / tổ chức là bắt buộc")]
        [MaxLength(150, ErrorMessage = "Tên tổ chức tối đa 150 ký tự")]
        public string OrganizationName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Họ tên người đại diện là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Họ tên người đại diện tối đa 100 ký tự")]
        public string RepresentativeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email người đại diện là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu quản trị là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
