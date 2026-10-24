using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO thông tin người dùng trong danh bạ quản trị Admin & CSKH 360
    /// </summary>
    public class AdminUserItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int Streak { get; set; }
        public int MaxStreak { get; set; }
        public int Hearts { get; set; } = 5;
        public int Xp { get; set; }
        public string Role { get; set; } = "LEARNER";
        public string Plan { get; set; } = "FREE"; // FREE | PRO | SCHOOL_LICENSE
        public string? SchoolName { get; set; }
        public string Status { get; set; } = "ACTIVE"; // ACTIVE | DISABLED | BANNED
        public string JoinedDate { get; set; } = string.Empty;
        public bool LostStreakRecently { get; set; }
        public string? ComplaintText { get; set; }
        public List<AdminUserHistoryDto> History { get; set; } = new();
    }

    /// <summary>
    /// Lịch sử học tập gần nhất của người dùng
    /// </summary>
    public class AdminUserHistoryDto
    {
        public string Date { get; set; } = string.Empty;
        public string Lesson { get; set; } = string.Empty;
        public int Xp { get; set; }
        public string Status { get; set; } = "PASS";
    }

    /// <summary>
    /// DTO yêu cầu tặng điểm XP CSKH
    /// </summary>
    public class RewardUserXpDto
    {
        public int Amount { get; set; } = 100;
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO yêu cầu cứu chuỗi Streak
    /// </summary>
    public class RescueStreakDto
    {
        public int? TargetStreak { get; set; }
    }
}
