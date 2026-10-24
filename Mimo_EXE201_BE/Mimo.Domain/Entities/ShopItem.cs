using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("ShopItems")]
    public class ShopItem
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int CostXp { get; set; }

        [MaxLength(50)]
        public string Icon { get; set; } = "🛍️";

        [MaxLength(30)]
        public string? Color { get; set; }

        [MaxLength(30)]
        public string? BgColor { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
