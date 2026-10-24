using System.Collections.Generic;

namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO tổng hợp dữ liệu doanh thu, bảng giá gói cước và cổng thanh toán
    /// </summary>
    public class AdminMonetizationDto
    {
        public long Mrr { get; set; } = 89500000;
        public long Arr { get; set; } = 1074000000;
        public double GrowthPercent { get; set; } = 18.5;
        public List<AdminPlanDto> Plans { get; set; } = new();
        public List<AdminPaymentGatewayDto> Gateways { get; set; } = new();
        public List<AdminTransactionDto> Transactions { get; set; } = new();
    }

    /// <summary>
    /// DTO gói cước B2C / B2B
    /// </summary>
    public class AdminPlanDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "B2C";
        public int? Price { get; set; }
        public int? OriginalPrice { get; set; }
        public int? PricePerSeat { get; set; }
        public string BillingCycle { get; set; } = "1 Tháng";
        public int ActiveSubscribers { get; set; }
        public int? ActiveOrganizations { get; set; }
        public int? TotalActiveSeats { get; set; }
        public List<string> Features { get; set; } = new();
    }

    /// <summary>
    /// DTO trạng thái cổng thanh toán tích hợp
    /// </summary>
    public class AdminPaymentGatewayDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "ONLINE";
        public string SuccessRate { get; set; } = "99.4%";
        public string Latency { get; set; } = "240ms";
    }

    /// <summary>
    /// DTO giao dịch nạp tiền / thanh toán gói
    /// </summary>
    public class AdminTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Customer { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Method { get; set; } = "VietQR";
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = "SUCCESS"; // SUCCESS | REFUNDED | FAILED
    }
}
