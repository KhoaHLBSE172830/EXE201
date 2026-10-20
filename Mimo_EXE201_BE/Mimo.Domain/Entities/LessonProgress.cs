using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("LessonProgresses")]
    public class LessonProgress : BaseEntity
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        [Required]
        public Guid LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        public int LastWatchedSec { get; set; } = 0;

        public LessonProgressStatusEnum Status { get; set; } = LessonProgressStatusEnum.NotStarted;

        public float? Score { get; set; }
    }
}
