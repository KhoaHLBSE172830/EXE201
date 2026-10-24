using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("ShopPurchases")]
    public class ShopPurchase
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
        public string ItemId { get; set; } = string.Empty;

        [ForeignKey("ItemId")]
        public virtual ShopItem ShopItem { get; set; } = null!;

        public int CostXp { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    }
}
