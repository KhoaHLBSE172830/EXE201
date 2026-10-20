using FluentValidation;
using Mimo.Application.DTOs.User;

namespace Mimo.Application.Validators.User
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự.")
                .MaximumLength(100).WithMessage("Mật khẩu mới không được vượt quá 100 ký tự.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("Mật khẩu mới không được trùng với mật khẩu hiện tại.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu mới không được để trống.")
                .Equal(x => x.NewPassword)
                .WithMessage("Xác nhận mật khẩu mới không khớp với mật khẩu mới.");
        }
    }
}
