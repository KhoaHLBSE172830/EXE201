using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Enrollments")]
    public class Enrollment : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public float CompletionPercent { get; set; } = 0f;

        public EnrollmentStatusEnum Status { get; set; } = EnrollmentStatusEnum.Active;

        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
        public virtual Certificate? Certificate { get; set; }
    }
}
