using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("UserBadges")]
    public class UserBadge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string BadgeId { get; set; } = string.Empty;

        [ForeignKey("BadgeId")]
        public virtual Badge Badge { get; set; } = null!;

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}
