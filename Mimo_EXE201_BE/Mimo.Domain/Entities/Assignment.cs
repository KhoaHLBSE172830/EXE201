using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Assignments")]
    public class Assignment : BaseEntity
    {
        [Required]
        public Guid ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        public Guid NodeId { get; set; }

        [ForeignKey("NodeId")]
        public virtual Node Node { get; set; } = null!;

        [Required]
        public Guid CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        public DateTime Deadline { get; set; }

        public int PassScoreThreshold { get; set; } = 80;

        public int RewardXp { get; set; } = 50;

        public virtual ICollection<AssignmentProgress> AssignmentProgresses { get; set; } = new List<AssignmentProgress>();
        public virtual ICollection<LessonSession> LessonSessions { get; set; } = new List<LessonSession>();
    }
}
