using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;

namespace Mimo.Application.Services
{
    public class AdminMonetizationService : IAdminMonetizationService
    {
        private static readonly List<AdminTransactionDto> _inMemoryTransactions = new()
        {
            new AdminTransactionDto
            {
                Id = "tx_9812",
                Customer = "Trần Văn Hoàng",
                Plan = "MIMO Plus Năm",
                Amount = 699000,
                Method = "VietQR",
                Time = "2026-10-19 11:20",
                Status = "SUCCESS"
            },
            new AdminTransactionDto
            {
                Id = "tx_9811",
                Customer = "THPT FPT Cần Thơ",
                Plan = "B2B Gia hạn +50 Suất",
                Amount = 7500000,
                Method = "PayOS",
                Time = "2026-10-18 16:45",
                Status = "SUCCESS"
            },
            new AdminTransactionDto
            {
                Id = "tx_9810",
                Customer = "Lê Thu Trang",
                Plan = "MIMO Plus Tháng",
                Amount = 79000,
                Method = "MoMo",
                Time = "2026-10-18 14:10",
                Status = "SUCCESS"
            },
            new AdminTransactionDto
            {
                Id = "tx_9809",
                Customer = "Nguyễn Duy Mạnh",
                Plan = "MIMO Plus Tháng",
                Amount = 79000,
                Method = "ZaloPay",
                Time = "2026-10-17 21:05",
                Status = "REFUNDED"
            }
        };

        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly ILogger<AdminMonetizationService> _logger;

        public AdminMonetizationService(
            IGenericRepository<Organization> orgRepository,
            IGenericRepository<UserStats> statsRepository,
            ILogger<AdminMonetizationService> logger)
        {
            _orgRepository = orgRepository;
            _statsRepository = statsRepository;
            _logger = logger;
        }

        public async Task<AdminMonetizationDto> GetMonetizationAsync(CancellationToken cancellationToken = default)
        {
            var orgs = await _orgRepository.ListAsync();
            var totalActiveSeats = orgs.Sum(o => o.SeatCount);
            var proSubscribers = await _statsRepository.CountAsync(s => s.IsPro);

            long b2bMrr = totalActiveSeats > 0 ? (totalActiveSeats * 15000L) : 75000000L;
            long b2cMrr = Math.Max(proSubscribers * 79000L, 14500000L);
            long totalMrr = b2bMrr + b2cMrr;
            long totalArr = totalMrr * 12;

            var plans = new List<AdminPlanDto>
            {
                new AdminPlanDto
                {
                    Id = "plan_b2c_monthly",
                    Name = "MIMO Premium 1 Tháng (Monthly)",
                    Type = "B2C",
                    Price = 79000,
                    OriginalPrice = 99000,
                    BillingCycle = "1 Tháng",
                    ActiveSubscribers = Math.Max(proSubscribers, 340),
                    Features = new List<string> { "Toàn bộ tình huống PCCC & Sơ cứu", "Vô hạn Tim", "Giải thích sâu của Bác sĩ AI", "Không quảng cáo" }
                },
                new AdminPlanDto
                {
                    Id = "plan_b2c_quarterly",
                    Name = "MIMO Premium 3 Tháng (Quarterly - Chủ Lực)",
                    Type = "B2C",
                    Price = 219000,
                    OriginalPrice = 237000,
                    BillingCycle = "3 Tháng",
                    ActiveSubscribers = 420,
                    Features = new List<string> { "Vô hạn Tim", "Báo cáo phụ huynh hàng tuần", "Bùa bảo vệ Streak tự động", "Tiết kiệm 18.000đ" }
                },
                new AdminPlanDto
                {
                    Id = "plan_b2c_halfyear",
                    Name = "MIMO Premium 6 Tháng (Half-Year)",
                    Type = "B2C",
                    Price = 399000,
                    OriginalPrice = 474000,
                    BillingCycle = "6 Tháng",
                    ActiveSubscribers = 185,
                    Features = new List<string> { "Vô hạn Tim", "Chứng chỉ định danh QR PCCC", "Hỗ trợ ưu tiên 1:1 chuyên gia", "Tiết kiệm 75.000đ" }
                },
                new AdminPlanDto
                {
                    Id = "plan_b2b_school",
                    Name = "MIMO School & Enterprise",
                    Type = "B2B",
                    PricePerSeat = 25000,
                    BillingCycle = "Hợp đồng Niên khóa",
                    ActiveOrganizations = Math.Max(orgs.Count, 4),
                    TotalActiveSeats = Math.Max(totalActiveSeats, 5210),
                    Features = new List<string> { "Bảng quản trị Trường học", "Mã giáo viên không giới hạn", "Báo cáo Red Flags toàn trường", "Chứng chỉ PCCC kiểm định" }
                }
            };

            var gateways = new List<AdminPaymentGatewayDto>
            {
                new AdminPaymentGatewayDto { Name = "PayOS (VietQR Pro)", Status = "ONLINE", SuccessRate = "99.4%", Latency = "240ms" },
                new AdminPaymentGatewayDto { Name = "MoMo Payment Gateway", Status = "ONLINE", SuccessRate = "98.8%", Latency = "310ms" },
                new AdminPaymentGatewayDto { Name = "ZaloPay Merchant", Status = "ONLINE", SuccessRate = "99.1%", Latency = "280ms" },
                new AdminPaymentGatewayDto { Name = "VNPay QR Gateway", Status = "ONLINE", SuccessRate = "99.5%", Latency = "210ms" }
            };

            return new AdminMonetizationDto
            {
                Mrr = totalMrr,
                Arr = totalArr,
                GrowthPercent = 18.5,
                Plans = plans,
                Gateways = gateways,
                Transactions = _inMemoryTransactions.ToList()
            };
        }

        public Task<bool> RefundTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            var tx = _inMemoryTransactions.FirstOrDefault(t => t.Id == transactionId);
            if (tx != null)
            {
                tx.Status = "REFUNDED";
                _logger.LogInformation("Admin đã thực hiện hoàn tiền (Refund) cho mã giao dịch {TransactionId}", transactionId);
            }

            return Task.FromResult(true);
        }
    }
}
