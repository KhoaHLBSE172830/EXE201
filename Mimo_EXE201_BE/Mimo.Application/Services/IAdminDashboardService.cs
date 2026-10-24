using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminAnalyticsDto> GetAnalyticsAsync(CancellationToken cancellationToken = default);
        Task<List<AdminSkillGapDto>> GetSkillGapsAsync(CancellationToken cancellationToken = default);
        Task<bool> BroadcastAlertAsync(BroadcastAlertRequestDto request, CancellationToken cancellationToken = default);
    }
}
