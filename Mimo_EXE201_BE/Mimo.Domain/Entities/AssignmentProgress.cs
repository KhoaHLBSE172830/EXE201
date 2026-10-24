using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("AssignmentProgress")]
    public class AssignmentProgress : BaseEntity
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;

        public Guid? SessionId { get; set; }

        [ForeignKey("SessionId")]
        public virtual LessonSession? Session { get; set; }

        public int BestScore { get; set; } = 0;

        public bool IsPassed { get; set; } = false;

        public DateTime? CompletedAt { get; set; }
    }
}
