using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminUserService
    {
        Task<List<AdminUserItemDto>> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<List<AdminUserItemDto>> RescueStreakAsync(Guid userId, int? targetStreak = null, CancellationToken cancellationToken = default);
        Task<List<AdminUserItemDto>> RefillHeartsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<AdminUserItemDto>> RewardXpAsync(Guid userId, int amount, Guid adminUserId, CancellationToken cancellationToken = default);
        Task<List<AdminUserItemDto>> ToggleBanAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
