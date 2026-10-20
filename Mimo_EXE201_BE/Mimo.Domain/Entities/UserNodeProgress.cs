using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("UserNodeProgresses")]
    public class UserNodeProgress : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid NodeId { get; set; }

        [ForeignKey("NodeId")]
        public virtual Node Node { get; set; } = null!;

        /// <summary>
        /// locked | current | done
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "locked";

        public DateTime? CompletedAt { get; set; }
    }
}
