using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Lessons")]
    public class Lesson : BaseEntity
    {
        [Required]
        public Guid ModuleId { get; set; }

        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public LessonContentTypeEnum ContentType { get; set; } = LessonContentTypeEnum.InteractiveSimulation;

        public int DurationSeconds { get; set; } = 0;

        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
