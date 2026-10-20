using FluentValidation;
using Mimo.Application.DTOs.Auth;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống.")
                .Length(2, 100).WithMessage("Họ và tên phải từ 2 đến 100 ký tự.")
                .Must(name => !name.Contains('<') && !name.Contains('>'))
                .WithMessage("Họ và tên không được chứa ký tự HTML độc hại.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.")
                .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Vai trò người dùng không hợp lệ.")
                .Must(role => role != UserRoleEnum.Admin)
                .WithMessage("Không thể tự đăng ký tài khoản với vai trò Quản trị viên (Admin).");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(0|\+84)[0-9]{9,10}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Số điện thoại không đúng định dạng (Ví dụ: 0912345678 hoặc +84912345678).");

            RuleFor(x => x.OrganizationName)
                .NotEmpty()
                .When(x => x.Role == UserRoleEnum.Organization)
                .WithMessage("Tên tổ chức/trường học là bắt buộc khi đăng ký với vai trò Organization.")
                .Length(3, 150)
                .When(x => x.Role == UserRoleEnum.Organization)
                .WithMessage("Tên tổ chức/trường học phải từ 3 đến 150 ký tự.");

            RuleFor(x => x.OrganizationCode)
                .MaximumLength(50).WithMessage("Mã tổ chức không được vượt quá 50 ký tự.")
                .Matches(@"^[a-zA-Z0-9_\-]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.OrganizationCode))
                .WithMessage("Mã tổ chức chỉ được chứa chữ cái, số và dấu gạch.");
        }
    }
}
