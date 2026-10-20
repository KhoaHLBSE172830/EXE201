using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("LessonSessions")]
    public class LessonSession : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid NodeId { get; set; }

        [ForeignKey("NodeId")]
        public virtual Node Node { get; set; } = null!;

        public Guid? AssignmentId { get; set; }

        /// <summary>
        /// in_progress | completed | failed
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "in_progress";

        public int HeartsLost { get; set; } = 0;

        public int TotalXpEarned { get; set; } = 0;

        public int CurrentComboStreak { get; set; } = 0;

        public int MaxComboStreak { get; set; } = 0;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(30);

        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<SessionAnswer> Answers { get; set; } = new List<SessionAnswer>();
    }
}
