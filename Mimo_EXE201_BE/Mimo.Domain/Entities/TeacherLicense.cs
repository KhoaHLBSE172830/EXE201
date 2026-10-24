using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("TeacherLicenses")]
    public class TeacherLicense : BaseEntity
    {
        [Required]
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;

        [Required]
        [MaxLength(32)]
        public string ActivationCode { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Note { get; set; }

        [Column(TypeName = "jsonb")]
        public string AssignedClassesJson { get; set; } = "[]";

        public Guid? ClaimedByEducatorId { get; set; }

        [ForeignKey("ClaimedByEducatorId")]
        public virtual User? ClaimedByEducator { get; set; }

        public bool IsClaimed { get; set; } = false;

        public DateTime ExpiresAt { get; set; }
    }
}
