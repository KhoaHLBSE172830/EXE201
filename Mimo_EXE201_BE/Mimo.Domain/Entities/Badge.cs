using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Badges")]
    public class Badge
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "general";

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Icon { get; set; } = "🎖️";

        [MaxLength(50)]
        public string Rarity { get; set; } = "Phổ biến";

        /// <summary>
        /// CompletedTopic, StreakDays, PerfectLessons, TotalXp, FirstLesson
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CriteriaType { get; set; } = string.Empty;

        public int CriteriaValue { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
