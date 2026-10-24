using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Gamification
{
    public class QuestDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string QuestType { get; set; } = string.Empty;
        public int Target { get; set; }
        public int CurrentProgress { get; set; }
        public string RewardType { get; set; } = "XP";
        public int RewardAmount { get; set; }
        public string Icon { get; set; } = "🎯";
        public bool IsCompleted { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime? ClaimedAt { get; set; }
    }

    public class DailyQuestsResponseDto
    {
        public string DateKey { get; set; } = string.Empty;
        public int ResetInSeconds { get; set; }
        public List<QuestDto> Quests { get; set; } = new();
    }

    public class ClaimQuestResponseDto
    {
        public Guid QuestId { get; set; }
        public string RewardType { get; set; } = "XP";
        public int RewardAmount { get; set; }
        public int NewTotalXp { get; set; }
        public int Hearts { get; set; }
        public int StreakFreezeCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ShopItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CostXp { get; set; }
        public string Icon { get; set; } = "🛍️";
        public string? Color { get; set; }
        public string? BgColor { get; set; }
        public bool CanAfford { get; set; }
    }

    public class ShopItemsResponseDto
    {
        public int TotalXp { get; set; }
        public int Hearts { get; set; }
        public int StreakFreezeCount { get; set; }
        public bool IsPro { get; set; }
        public List<ShopItemDto> Items { get; set; } = new();
    }

    public class PurchaseItemRequestDto
    {
        public string ItemId { get; set; } = string.Empty;
    }

    public class PurchaseItemResponseDto
    {
        public string ItemId { get; set; } = string.Empty;
        public int CostXp { get; set; }
        public int NewTotalXp { get; set; }
        public int Hearts { get; set; }
        public int StreakFreezeCount { get; set; }
        public bool IsPro { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BadgeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "🎖️";
        public string Rarity { get; set; } = "Phổ biến";
        public string CriteriaType { get; set; } = string.Empty;
        public int CriteriaValue { get; set; }
        public int ProgressPercent { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }

    public class BadgesResponseDto
    {
        public int TotalBadges { get; set; }
        public int UnlockedBadgesCount { get; set; }
        public List<BadgeDto> Badges { get; set; } = new();
    }

    public class EmergencyDialDto
    {
        public string Name { get; set; } = string.Empty;
        public string Dial { get; set; } = string.Empty;
    }

    public class HandbookArticleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Pillar { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Badge { get; set; } = string.Empty;
        public string ReadTime { get; set; } = string.Empty;
        public List<EmergencyDialDto>? EmergencyDials { get; set; }
        public string KeyProtocol { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = string.Empty;
        public string ContentMarkdown { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class HandbookResponseDto
    {
        public List<string> Pillars { get; set; } = new();
        public List<EmergencyDialDto> EmergencyHotlines { get; set; } = new();
        public List<HandbookArticleDto> Articles { get; set; } = new();
    }

    public class DailyStudyStatDto
    {
        public string DayOfWeek { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int Minutes { get; set; }
        public int Xp { get; set; }
    }

    public class DomainMasteryDto
    {
        public Guid DomainId { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int TotalNodes { get; set; }
        public int CompletedNodes { get; set; }
        public int MasteryPercent { get; set; }
    }

    public class StudyStatsResponseDto
    {
        public int TotalMinutesLearned { get; set; }
        public int TotalLessonsCompleted { get; set; }
        public int TotalXp { get; set; }
        public int CurrentStreak { get; set; }
        public List<DailyStudyStatDto> WeeklyStats { get; set; } = new();
        public List<DomainMasteryDto> SkillRadar { get; set; } = new();
        public List<BadgeDto> RecentBadges { get; set; } = new();
    }
}
