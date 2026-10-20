using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("SessionAnswers")]
    public class SessionAnswer : BaseEntity
    {
        [Required]
        public Guid SessionId { get; set; }

        [ForeignKey("SessionId")]
        public virtual LessonSession Session { get; set; } = null!;

        [Required]
        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        [Required]
        [Column(TypeName = "jsonb")]
        public string SubmittedAnswer { get; set; } = "{}";

        public bool IsCorrect { get; set; }

        public int TimeSpentSeconds { get; set; } = 0;

        public int XpEarned { get; set; } = 0;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
