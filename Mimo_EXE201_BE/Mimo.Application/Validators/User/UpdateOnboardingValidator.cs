using System.Text.Json;
using FluentValidation;
using Mimo.Application.DTOs.User;

namespace Mimo.Application.Validators.User
{
    public class UpdateOnboardingValidator : AbstractValidator<UpdateOnboardingDto>
    {
        public UpdateOnboardingValidator()
        {
            RuleFor(x => x.DailyGoalMinutes)
                .InclusiveBetween(5, 60)
                .WithMessage("Mục tiêu học tập hàng ngày phải nằm trong khoảng từ 5 đến 60 phút (khuyến nghị: 5, 10, 15, 20 phút).");

            RuleFor(x => x.RiskSurveyResult)
                .Must(BeValidJson)
                .When(x => !string.IsNullOrWhiteSpace(x.RiskSurveyResult))
                .WithMessage("Kết quả khảo sát nhận diện rủi ro phải là định dạng chuỗi JSON hợp lệ.");
        }

        private static bool BeValidJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return true;
            try
            {
                using var doc = JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
