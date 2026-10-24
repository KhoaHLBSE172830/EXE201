using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("QuestionFeedbacks")]
    public class QuestionFeedback : BaseEntity
    {
        [Required]
        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public Guid? ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual User? Reviewer { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReviewerName { get; set; } = string.Empty;

        /// <summary>
        /// Reject | Expert
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Kind { get; set; } = "Reject";

        [Required]
        [Column(TypeName = "text")]
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// Pending | Resolved
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? ResolvedAt { get; set; }
    }
}
