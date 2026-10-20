using FluentValidation;
using Mimo.Application.DTOs.Auth;

namespace Mimo.Application.Validators.Auth
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email hoặc tên đăng nhập không được để trống.")
                .MaximumLength(255).WithMessage("Tài khoản không được vượt quá 255 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.")
                .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự.");
        }
    }
}
