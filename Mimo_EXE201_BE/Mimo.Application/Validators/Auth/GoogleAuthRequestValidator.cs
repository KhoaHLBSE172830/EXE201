using FluentValidation;
using Mimo.Application.DTOs.Auth;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Validators.Auth
{
    public class GoogleAuthRequestValidator : AbstractValidator<GoogleAuthRequestDto>
    {
        public GoogleAuthRequestValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("Google ID Token không được để trống.")
                .MinimumLength(5).WithMessage("Google ID Token không hợp lệ.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Vai trò người dùng không hợp lệ.")
                .Must(role => role != UserRoleEnum.Admin)
                .WithMessage("Không thể gán vai trò Admin khi đăng nhập bằng Google.");
        }
    }
}
