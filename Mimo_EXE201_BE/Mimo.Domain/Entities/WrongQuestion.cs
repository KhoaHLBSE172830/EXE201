using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("WrongQuestions")]
    public class WrongQuestion : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public int TimesWrong { get; set; } = 1;

        public DateTime? ResolvedAt { get; set; }

        public DateTime LastAttemptAt { get; set; } = DateTime.UtcNow;
    }
}
