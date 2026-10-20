using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Transactions")]
    public class Transaction : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [Required]
        public Guid CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;

        [MaxLength(10)]
        public string Currency { get; set; } = "VND";

        public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.VNPay;

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public TransactionStatusEnum Status { get; set; } = TransactionStatusEnum.Pending;
    }
}
