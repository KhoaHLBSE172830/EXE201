using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("UserQuests")]
    public class UserQuest : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid QuestId { get; set; }

        [ForeignKey("QuestId")]
        public virtual DailyQuest Quest { get; set; } = null!;

        /// <summary>
        /// Date formatted in UTC+7 (Asia/Ho_Chi_Minh)
        /// </summary>
        [Required]
        public DateOnly DateKey { get; set; }

        public int CurrentProgress { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;

        public bool IsClaimed { get; set; } = false;

        public DateTime? ClaimedAt { get; set; }
    }
}
