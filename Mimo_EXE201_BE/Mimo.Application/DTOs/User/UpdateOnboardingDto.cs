using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.User
{
    public class UpdateOnboardingDto
    {
        [Required(ErrorMessage = "Mục tiêu hàng ngày là bắt buộc")]
        [Range(5, 60, ErrorMessage = "Mục tiêu học tập hàng ngày từ 5 đến 60 phút")]
        public int DailyGoalMinutes { get; set; } = 10;

        public string? RiskSurveyResult { get; set; }
    }
}
