using System;
using System.ComponentModel.DataAnnotations;

namespace Mimo.Application.DTOs.Payment
{
    public class CreatePaymentRequestDto
    {
        [Required(ErrorMessage = "PlanCode hoặc CourseId là bắt buộc")]
        public string? PlanCode { get; set; }

        public Guid? CourseId { get; set; }

        public string? OrderInfo { get; set; }
    }

    public class PaymentUrlResponseDto
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string TxnRef { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
    }

    public class PaymentResultDto
    {
        public string TxnRef { get; set; } = string.Empty;
        public string? VnpayTransactionNo { get; set; }
        public string? BankCode { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? PlanCode { get; set; }
        public bool IsPro { get; set; }
        public DateTime? ProExpiresAt { get; set; }
        public int StreakFreezeAdded { get; set; }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TxnRef { get; set; } = string.Empty;
        public string? PlanCode { get; set; }
        public Guid? CourseId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public string PaymentMethod { get; set; } = "VNPay";
        public string Status { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? OrderInfo { get; set; }
    }
}
