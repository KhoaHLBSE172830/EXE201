using System;

namespace Mimo.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Message { get; set; } = "Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.";
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
