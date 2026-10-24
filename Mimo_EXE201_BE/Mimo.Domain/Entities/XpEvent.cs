using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("XpEvents")]
    public class XpEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int Amount { get; set; }

        /// <summary>
        /// lesson_correct, combo_bonus, perfect_lesson, chest_reward, practice_reward
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Source { get; set; } = string.Empty;

        public Guid? ReferenceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
