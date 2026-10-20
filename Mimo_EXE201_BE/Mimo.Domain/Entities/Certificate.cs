using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Certificates")]
    public class Certificate : BaseEntity
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(1000)]
        public string PdfUrl { get; set; } = string.Empty;
    }
}
