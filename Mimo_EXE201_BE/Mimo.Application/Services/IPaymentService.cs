using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Mimo.Application.DTOs.Payment;

namespace Mimo.Application.Services
{
    public interface IPaymentService
    {
        Task<PaymentUrlResponseDto> CreateVnPayPaymentUrlAsync(Guid userId, CreatePaymentRequestDto request, string ipAddress, CancellationToken cancellationToken = default);
        Task<PaymentResultDto> ProcessVnPayCallbackAsync(IQueryCollection queryParams, CancellationToken cancellationToken = default);
        Task<(string code, string message)> ProcessVnPayIpnAsync(IQueryCollection queryParams, CancellationToken cancellationToken = default);
        Task<PaymentResultDto> MockActivatePlanAsync(Guid userId, string planCode, CancellationToken cancellationToken = default);
        Task<List<TransactionDto>> GetUserTransactionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
