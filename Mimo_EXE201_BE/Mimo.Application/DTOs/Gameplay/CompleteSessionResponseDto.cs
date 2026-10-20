using System;
using Mimo.Application.DTOs.Streak;

namespace Mimo.Application.DTOs.Gameplay
{
    public class CompleteSessionResponseDto
    {
        public Guid SessionId { get; set; }

        public Guid NodeId { get; set; }

        public string Status { get; set; } = "completed";

        public int TotalXpEarned { get; set; }

        public int HeartsLost { get; set; }

        public int HeartsRemaining { get; set; }

        public int MaxComboStreak { get; set; }

        public bool IsPerfectLesson { get; set; }

        public int PerfectBonusXp { get; set; }

        public StreakResultDto? StreakInfo { get; set; }

        public Guid? UnlockedNextNodeId { get; set; }
    }
}
