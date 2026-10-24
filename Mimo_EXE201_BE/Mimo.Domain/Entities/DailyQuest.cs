using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("DailyQuests")]
    public class DailyQuest : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// XpGoal, LessonsCount, PerfectLesson, PracticeSession
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string QuestType { get; set; } = string.Empty;

        public int Target { get; set; }

        /// <summary>
        /// XP, StreakFreeze, Heart
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RewardType { get; set; } = "XP";

        public int RewardAmount { get; set; }

        [MaxLength(50)]
        public string Icon { get; set; } = "🎯";

        public bool IsActive { get; set; } = true;
    }
}
