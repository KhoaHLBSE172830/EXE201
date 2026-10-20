using System;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.DTOs.User
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public UserStatusEnum Status { get; set; }
        public string AuthType { get; set; } = "Local";

        // Role & Organization
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Guid? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }

        // Timezone
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

        // Gamification: XP & Level (Section 5.3)
        public int TotalXp { get; set; }
        public int Level { get; set; }
        public string LevelTitle { get; set; } = string.Empty;
        public int XpToNextLevel { get; set; }
        public double LevelProgressPercent { get; set; }

        // Gamification: Hearts (Section 5.1)
        public int Hearts { get; set; } = 5;
        public int MaxHearts { get; set; } = 5;
        public int NextHeartInSeconds { get; set; }
        public bool IsPro { get; set; }
        public DateTime? ProExpiresAt { get; set; }

        // Gamification: Streak & Freeze
        public int Streak { get; set; }
        public int StreakFreezeCount { get; set; }
        public DateOnly? LastActiveDate { get; set; }

        // Onboarding
        public int DailyGoalMinutes { get; set; } = 10;
        public string? RiskSurveyResult { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
