using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminMonetizationService
    {
        Task<AdminMonetizationDto> GetMonetizationAsync(CancellationToken cancellationToken = default);
        Task<bool> RefundTransactionAsync(string transactionId, CancellationToken cancellationToken = default);
    }
}
