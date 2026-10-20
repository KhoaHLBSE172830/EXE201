using System;
using FluentValidation;
using Mimo.Application.DTOs.User;

namespace Mimo.Application.Validators.User
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống.")
                .Length(2, 100).WithMessage("Họ và tên phải từ 2 đến 100 ký tự.")
                .Must(name => !name.Contains('<') && !name.Contains('>'))
                .WithMessage("Họ và tên không được chứa ký tự HTML độc hại.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(0|\+84)[0-9]{9,10}$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Số điện thoại không đúng định dạng (Ví dụ: 0912345678 hoặc +84912345678).");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Địa chỉ không được vượt quá 200 ký tự.");

            RuleFor(x => x.AvatarUrl)
                .Must(BeValidUrl)
                .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl))
                .WithMessage("Đường dẫn ảnh đại diện (AvatarUrl) phải là URL hợp lệ (http:// hoặc https://).");

            RuleFor(x => x.Timezone)
                .Must(BeValidTimezone)
                .When(x => !string.IsNullOrWhiteSpace(x.Timezone))
                .WithMessage("Múi giờ không hợp lệ (Ví dụ: Asia/Ho_Chi_Minh, UTC).");
        }

        private static bool BeValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static bool BeValidTimezone(string? tz)
        {
            if (string.IsNullOrWhiteSpace(tz)) return true;
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(tz);
                return true;
            }
            catch
            {
                return tz.Equals("Asia/Ho_Chi_Minh", StringComparison.OrdinalIgnoreCase) ||
                       tz.Equals("UTC", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
