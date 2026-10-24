using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("ClassMembers")]
    public class ClassMember : BaseEntity
    {
        [Required]
        public Guid ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Enrolled";
    }
}
