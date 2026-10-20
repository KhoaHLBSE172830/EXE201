using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Courses")]
    public class Course : BaseEntity
    {
        [Required]
        public Guid InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public virtual User Instructor { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        public DifficultyLevelEnum DifficultyLevel { get; set; } = DifficultyLevelEnum.Beginner;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        public CourseStatusEnum Status { get; set; } = CourseStatusEnum.Draft;

        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
