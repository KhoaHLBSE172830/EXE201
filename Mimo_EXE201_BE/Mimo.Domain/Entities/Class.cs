using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Classes")]
    public class Class : BaseEntity
    {
        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        public Guid? EducatorId { get; set; }

        [ForeignKey("EducatorId")]
        public virtual User? Educator { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ClassCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Grade { get; set; }

        [MaxLength(50)]
        public string? Room { get; set; }

        [MaxLength(150)]
        public string? FocusTopic { get; set; }

        public int MaxStudents { get; set; } = 50;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<ClassMember> ClassMembers { get; set; } = new List<ClassMember>();
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
