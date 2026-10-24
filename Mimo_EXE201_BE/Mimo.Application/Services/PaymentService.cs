using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimo.Application.DTOs.Payment;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using Mimo.Application.Helper.Payment;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Course> _courseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly VnPayOptions _vnPayOptions;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IGenericRepository<Transaction> transactionRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Course> courseRepository,
            IUnitOfWork unitOfWork,
            IOptions<VnPayOptions> vnPayOptions,
            ILogger<PaymentService> logger)
        {
            _transactionRepository = transactionRepository;
            _statsRepository = statsRepository;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _unitOfWork = unitOfWork;
            _vnPayOptions = vnPayOptions.Value;
            _logger = logger;
        }

        public async Task<PaymentUrlResponseDto> CreateVnPayPaymentUrlAsync(
            Guid userId,
            CreatePaymentRequestDto request,
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin tài khoản người dùng.");
            }

            decimal amount = 0;
            string orderInfo = "";
            string rawPlanCode = request.PlanCode?.Trim().ToUpperInvariant() ?? "";
            string planCode = rawPlanCode;
            if (planCode.StartsWith("MIMO_"))
            {
                planCode = planCode.Substring(5);
            }

            if (!string.IsNullOrEmpty(planCode))
            {
                switch (planCode)
                {
                    case "PREMIUM_1M":
                        amount = 79000;
                        orderInfo = $"MIMO - Nang cap goi Pro 1 Thang ({user.Email})";
                        break;
                    case "PREMIUM_3M":
                        amount = 219000;
                        orderInfo = $"MIMO - Nang cap goi Pro 3 Thang ({user.Email})";
                        break;
                    case "PREMIUM_6M":
                        amount = 399000;
                        orderInfo = $"MIMO - Nang cap goi Pro 6 Thang ({user.Email})";
                        break;
                    default:
                        throw new BadRequestException($"Gói đăng ký '{request.PlanCode}' không hợp lệ.");
                }
            }
            else if (request.CourseId.HasValue)
            {
                var course = await _courseRepository.FirstOrDefaultAsync(c => c.Id == request.CourseId.Value);
                if (course == null)
                {
                    throw new NotFoundException("Không tìm thấy khóa học yêu cầu thanh toán.");
                }

                amount = course.Price;
                orderInfo = $"MIMO - Mua khoa hoc {course.Title} ({user.Email})";
            }
            else
            {
                throw new BadRequestException("Cần cung cấp PlanCode hoặc CourseId để thanh toán.");
            }

            if (!string.IsNullOrEmpty(request.OrderInfo))
            {
                orderInfo = request.OrderInfo;
            }

            // Sinh mã tham chiếu giao dịch độc nhất
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var randomSuffix = Guid.NewGuid().ToString("N")[..8];
            var txnRef = $"{timestamp}_{randomSuffix}";

            // Dùng UnitOfWork bọc Transaction tạo bản ghi Pending
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = new Transaction
                {
                    UserId = userId,
                    CourseId = request.CourseId,
                    PlanCode = string.IsNullOrEmpty(planCode) ? null : planCode,
                    TxnRef = txnRef,
                    OrderInfo = orderInfo,
                    Amount = amount,
                    Currency = "VND",
                    PaymentMethod = PaymentMethodEnum.VNPay,
                    Status = TransactionStatusEnum.Pending,
                    TransactionDate = DateTime.UtcNow
                };

                await _transactionRepository.CreateAsync(transaction);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Khởi tạo giao dịch VNPay thành công. TxnRef: {TxnRef}, UserId: {UserId}, Amount: {Amount}",
                    txnRef, userId, amount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu giao dịch Transaction vào Database. TxnRef: {TxnRef}", txnRef);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            // Xây dựng URL thanh toán VNPay
            var vnPay = new VnPayLibrary();
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            vnPay.AddRequestData("vnp_Version", _vnPayOptions.Version);
            vnPay.AddRequestData("vnp_Command", _vnPayOptions.Command);
            vnPay.AddRequestData("vnp_TmnCode", _vnPayOptions.TmnCode);
            vnPay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString(CultureInfo.InvariantCulture));
            vnPay.AddRequestData("vnp_CreateDate", nowVn.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
            vnPay.AddRequestData("vnp_CurrCode", _vnPayOptions.CurrCode);
            vnPay.AddRequestData("vnp_IpAddr", string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress);
            vnPay.AddRequestData("vnp_Locale", _vnPayOptions.Locale);
            vnPay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnPay.AddRequestData("vnp_OrderType", "other");
            vnPay.AddRequestData("vnp_ReturnUrl", _vnPayOptions.ReturnUrl);
            vnPay.AddRequestData("vnp_TxnRef", txnRef);
            vnPay.AddRequestData("vnp_ExpireDate", nowVn.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));

            var paymentUrl = vnPay.CreateRequestUrl(_vnPayOptions.BaseUrl, _vnPayOptions.HashSecret);

            return new PaymentUrlResponseDto
            {
                PaymentUrl = paymentUrl,
                TxnRef = txnRef,
                Amount = amount,
                PlanCode = planCode,
                OrderInfo = orderInfo
            };
        }

        public async Task<PaymentResultDto> ProcessVnPayCallbackAsync(
            IQueryCollection queryParams,
            CancellationToken cancellationToken = default)
        {
            var vnPay = new VnPayLibrary();
            foreach (var key in queryParams.Keys)
            {
                var value = queryParams[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var inputHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = vnPay.ValidateSignature(inputHash, _vnPayOptions.HashSecret);
            if (!isValidSignature)
            {
                _logger.LogWarning("Chữ ký VNPay Callback không hợp lệ.");
                throw new BadRequestException("Chữ ký VNPay không hợp lệ.");
            }

            var txnRef = vnPay.GetResponseData("vnp_TxnRef");
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpTransactionNo = vnPay.GetResponseData("vnp_TransactionNo");
            var vnpBankCode = vnPay.GetResponseData("vnp_BankCode");
            var vnpAmountStr = vnPay.GetResponseData("vnp_Amount");

            long.TryParse(vnpAmountStr, out var vnpAmount);
            var amount = vnpAmount / 100;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = await _transactionRepository.FirstOrDefaultAsync(t => t.TxnRef == txnRef);
                if (transaction == null)
                {
                    _logger.LogWarning("Không tìm thấy giao dịch với TxnRef: {TxnRef}", txnRef);
                    throw new NotFoundException($"Không tìm thấy giao dịch với mã tham chiếu '{txnRef}'.");
                }

                // Nếu giao dịch đã hoàn tất trước đó (Idempotency)
                if (transaction.Status == TransactionStatusEnum.Success)
                {
                    var userStatsExisting = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == transaction.UserId);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    return new PaymentResultDto
                    {
                        TxnRef = transaction.TxnRef,
                        VnpayTransactionNo = transaction.VnpayTransactionNo,
                        BankCode = transaction.BankCode,
                        Amount = transaction.Amount,
                        Status = "Success",
                        Message = "Giao dịch đã được ghi nhận thành công trước đó.",
                        PlanCode = transaction.PlanCode,
                        IsPro = userStatsExisting?.IsPro ?? true,
                        ProExpiresAt = userStatsExisting?.ProExpiresAt,
                        StreakFreezeAdded = 0
                    };
                }

                transaction.VnpayResponseCode = vnpResponseCode;
                transaction.VnpayTransactionNo = vnpTransactionNo;
                transaction.BankCode = vnpBankCode;
                transaction.CompletedAt = DateTime.UtcNow;

                bool isSuccess = vnpResponseCode == "00";
                int streakFreezeAdded = 0;
                bool isPro = false;
                DateTime? proExpiresAt = null;

                if (isSuccess)
                {
                    transaction.Status = TransactionStatusEnum.Success;

                    // Kích hoạt hoặc gia hạn gói Mimo Pro cho người dùng
                    if (!string.IsNullOrEmpty(transaction.PlanCode))
                    {
                        var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == transaction.UserId);
                        if (stats == null)
                        {
                            stats = new UserStats
                            {
                                UserId = transaction.UserId,
                                Hearts = 5,
                                HeartsUpdatedAt = DateTime.UtcNow,
                                Streak = 0,
                                StreakFreezeCount = 0
                            };
                            await _statsRepository.CreateAsync(stats);
                        }

                        var cleanPlanCode = transaction.PlanCode?.ToUpperInvariant() ?? "";
                        if (cleanPlanCode.StartsWith("MIMO_")) cleanPlanCode = cleanPlanCode.Substring(5);

                        int daysToAdd = 30;
                        if (cleanPlanCode == "PREMIUM_3M")
                        {
                            daysToAdd = 90;
                            stats.StreakFreezeCount += 1;
                            streakFreezeAdded = 1;
                        }
                        else if (cleanPlanCode == "PREMIUM_6M")
                        {
                            daysToAdd = 180;
                            stats.StreakFreezeCount += 2;
                            streakFreezeAdded = 2;
                        }

                        stats.IsPro = true;
                        // Gia hạn cộng dồn nếu gói cũ còn hiệu lực
                        if (stats.ProExpiresAt.HasValue && stats.ProExpiresAt.Value > DateTime.UtcNow)
                        {
                            stats.ProExpiresAt = stats.ProExpiresAt.Value.AddDays(daysToAdd);
                        }
                        else
                        {
                            stats.ProExpiresAt = DateTime.UtcNow.AddDays(daysToAdd);
                        }

                        stats.Hearts = 5;
                        stats.HeartsUpdatedAt = DateTime.UtcNow;

                        await _statsRepository.UpdateAsync(stats);

                        isPro = stats.IsPro;
                        proExpiresAt = stats.ProExpiresAt;

                        _logger.LogInformation("Kích hoạt Mimo Pro thành công cho User: {UserId}. Plan: {Plan}, ProExpiresAt: {ExpiresAt}, Bùa Freeze tặng: {Freeze}",
                            transaction.UserId, transaction.PlanCode, proExpiresAt, streakFreezeAdded);
                    }
                }
                else
                {
                    transaction.Status = TransactionStatusEnum.Failed;
                    _logger.LogWarning("Thanh toán VNPay thất bại hoặc bị hủy. TxnRef: {TxnRef}, ResponseCode: {Code}",
                        txnRef, vnpResponseCode);
                }

                await _transactionRepository.UpdateAsync(transaction);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new PaymentResultDto
                {
                    TxnRef = txnRef,
                    VnpayTransactionNo = vnpTransactionNo,
                    BankCode = vnpBankCode,
                    Amount = amount,
                    Status = isSuccess ? "Success" : "Failed",
                    Message = isSuccess
                        ? "Thanh toán thành công! Gói Mimo Pro đã được kích hoạt."
                        : "Thanh toán không thành công hoặc giao dịch đã bị hủy bởi người dùng.",
                    PlanCode = transaction.PlanCode,
                    IsPro = isPro,
                    ProExpiresAt = proExpiresAt,
                    StreakFreezeAdded = streakFreezeAdded
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý Callback giao dịch VNPay. TxnRef: {TxnRef}", txnRef);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<(string code, string message)> ProcessVnPayIpnAsync(
            IQueryCollection queryParams,
            CancellationToken cancellationToken = default)
        {
            var vnPay = new VnPayLibrary();
            foreach (var key in queryParams.Keys)
            {
                var value = queryParams[key].ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            var inputHash = queryParams["vnp_SecureHash"].ToString();
            var isValidSignature = vnPay.ValidateSignature(inputHash, _vnPayOptions.HashSecret);
            if (!isValidSignature)
            {
                return ("97", "Invalid signature");
            }

            var txnRef = vnPay.GetResponseData("vnp_TxnRef");
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            var vnpTransactionNo = vnPay.GetResponseData("vnp_TransactionNo");
            var vnpBankCode = vnPay.GetResponseData("vnp_BankCode");
            var vnpAmountStr = vnPay.GetResponseData("vnp_Amount");

            long.TryParse(vnpAmountStr, out var vnpAmount);
            var amount = vnpAmount / 100;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = await _transactionRepository.FirstOrDefaultAsync(t => t.TxnRef == txnRef);
                if (transaction == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ("01", "Order not found");
                }

                if (transaction.Amount != amount)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ("04", "Invalid amount");
                }

                if (transaction.Status == TransactionStatusEnum.Success || transaction.Status == TransactionStatusEnum.Failed)
                {
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    return ("02", "Order already confirmed");
                }

                transaction.VnpayResponseCode = vnpResponseCode;
                transaction.VnpayTransactionNo = vnpTransactionNo;
                transaction.BankCode = vnpBankCode;
                transaction.CompletedAt = DateTime.UtcNow;

                if (vnpResponseCode == "00")
                {
                    transaction.Status = TransactionStatusEnum.Success;

                    if (!string.IsNullOrEmpty(transaction.PlanCode))
                    {
                        var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == transaction.UserId);
                        if (stats == null)
                        {
                            stats = new UserStats
                            {
                                UserId = transaction.UserId,
                                Hearts = 5,
                                HeartsUpdatedAt = DateTime.UtcNow,
                                Streak = 0,
                                StreakFreezeCount = 0
                            };
                            await _statsRepository.CreateAsync(stats);
                        }

                        var cleanPlanCode = transaction.PlanCode?.ToUpperInvariant() ?? "";
                        if (cleanPlanCode.StartsWith("MIMO_")) cleanPlanCode = cleanPlanCode.Substring(5);

                        int daysToAdd = 30;
                        if (cleanPlanCode == "PREMIUM_3M")
                        {
                            daysToAdd = 90;
                            stats.StreakFreezeCount += 1;
                        }
                        else if (cleanPlanCode == "PREMIUM_6M")
                        {
                            daysToAdd = 180;
                            stats.StreakFreezeCount += 2;
                        }

                        stats.IsPro = true;
                        if (stats.ProExpiresAt.HasValue && stats.ProExpiresAt.Value > DateTime.UtcNow)
                        {
                            stats.ProExpiresAt = stats.ProExpiresAt.Value.AddDays(daysToAdd);
                        }
                        else
                        {
                            stats.ProExpiresAt = DateTime.UtcNow.AddDays(daysToAdd);
                        }

                        stats.Hearts = 5;
                        stats.HeartsUpdatedAt = DateTime.UtcNow;

                        await _statsRepository.UpdateAsync(stats);
                    }
                }
                else
                {
                    transaction.Status = TransactionStatusEnum.Failed;
                }

                await _transactionRepository.UpdateAsync(transaction);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ("00", "Confirm Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý IPN VNPay cho TxnRef: {TxnRef}", txnRef);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ("99", "Unknown error");
            }
        }

        public async Task<List<TransactionDto>> GetUserTransactionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _transactionRepository.WhereAsync(t => t.UserId == userId);
            return transactions
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    TxnRef = t.TxnRef,
                    PlanCode = t.PlanCode,
                    CourseId = t.CourseId,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    PaymentMethod = t.PaymentMethod.ToString(),
                    Status = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    CompletedAt = t.CompletedAt,
                    OrderInfo = t.OrderInfo
                })
                .ToList();
        }

        public async Task<PaymentResultDto> MockActivatePlanAsync(
            Guid userId,
            string planCode,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            var cleanPlanCode = planCode?.Trim().ToUpperInvariant() ?? "";
            if (cleanPlanCode.StartsWith("MIMO_")) cleanPlanCode = cleanPlanCode.Substring(5);

            decimal amount = 79000;
            int daysToAdd = 30;
            int streakFreezeAdded = 0;
            string orderInfo = $"MIMO - Kích hoạt giả lập gói {cleanPlanCode} ({user.Email})";

            if (cleanPlanCode == "PREMIUM_3M")
            {
                amount = 219000;
                daysToAdd = 90;
                streakFreezeAdded = 1;
            }
            else if (cleanPlanCode == "PREMIUM_6M")
            {
                amount = 399000;
                daysToAdd = 180;
                streakFreezeAdded = 2;
            }
            else if (cleanPlanCode != "PREMIUM_1M")
            {
                throw new BadRequestException($"Mã gói cước '{planCode}' không hợp lệ.");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var randomSuffix = Guid.NewGuid().ToString("N")[..8];
            var txnRef = $"MOCK_{timestamp}_{randomSuffix}";

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = new Transaction
                {
                    UserId = userId,
                    PlanCode = cleanPlanCode,
                    TxnRef = txnRef,
                    OrderInfo = orderInfo,
                    Amount = amount,
                    Currency = "VND",
                    PaymentMethod = PaymentMethodEnum.VNPay,
                    Status = TransactionStatusEnum.Success,
                    TransactionDate = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    VnpayResponseCode = "00",
                    VnpayTransactionNo = $"MOCK_TXN_{randomSuffix}",
                    BankCode = "NCB"
                };

                await _transactionRepository.CreateAsync(transaction);

                var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
                if (stats == null)
                {
                    stats = new UserStats
                    {
                        UserId = userId,
                        Hearts = 5,
                        HeartsUpdatedAt = DateTime.UtcNow,
                        Streak = 0,
                        StreakFreezeCount = 0
                    };
                    await _statsRepository.CreateAsync(stats);
                }

                stats.IsPro = true;
                if (stats.ProExpiresAt.HasValue && stats.ProExpiresAt.Value > DateTime.UtcNow)
                {
                    stats.ProExpiresAt = stats.ProExpiresAt.Value.AddDays(daysToAdd);
                }
                else
                {
                    stats.ProExpiresAt = DateTime.UtcNow.AddDays(daysToAdd);
                }

                stats.StreakFreezeCount += streakFreezeAdded;
                stats.Hearts = 5;
                stats.HeartsUpdatedAt = DateTime.UtcNow;

                await _statsRepository.UpdateAsync(stats);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Kích hoạt giả lập gói Mimo Pro thành công vào DB. TxnRef: {TxnRef}, UserId: {UserId}, Plan: {Plan}",
                    txnRef, userId, cleanPlanCode);

                return new PaymentResultDto
                {
                    TxnRef = txnRef,
                    VnpayTransactionNo = transaction.VnpayTransactionNo,
                    BankCode = transaction.BankCode,
                    Amount = amount,
                    Status = "Success",
                    Message = $"Kích hoạt thành công gói {cleanPlanCode} trực tiếp vào Database.",
                    PlanCode = cleanPlanCode,
                    IsPro = true,
                    ProExpiresAt = stats.ProExpiresAt,
                    StreakFreezeAdded = streakFreezeAdded
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kích hoạt giả lập gói Mimo Pro. TxnRef: {TxnRef}", txnRef);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
