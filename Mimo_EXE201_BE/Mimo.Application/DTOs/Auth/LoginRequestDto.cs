using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Email hoặc Tên đăng nhập (UserName) là bắt buộc")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }
}
