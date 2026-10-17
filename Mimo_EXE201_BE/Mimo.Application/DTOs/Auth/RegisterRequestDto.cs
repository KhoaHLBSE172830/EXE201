using System.ComponentModel.DataAnnotations;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.DTOs.Auth
{
    public class RegisterRequestDto
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

        [Required(ErrorMessage = "Vai trò người dùng là bắt buộc")]
        [EnumDataType(typeof(UserRoleEnum), ErrorMessage = "Vai trò không hợp lệ (1: Admin, 2: Student, 3: Curator, 4: Organization)")]
        public UserRoleEnum Role { get; set; } = UserRoleEnum.Student;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Dành cho học sinh/giáo viên tham gia qua mã lớp/mã trường của Organization
        /// </summary>
        [MaxLength(50)]
        public string? OrganizationCode { get; set; }

        /// <summary>
        /// Dành cho trường hợp đăng ký tài khoản đại diện Organization mới
        /// </summary>
        [MaxLength(150)]
        public string? OrganizationName { get; set; }
    }
}
