using FluentValidation;
using Mimo.Application.DTOs.Auth;

namespace Mimo.Application.Validators.Auth
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh Token không được để trống.")
                .MinimumLength(16).WithMessage("Refresh Token không hợp lệ.");
        }
    }
}
