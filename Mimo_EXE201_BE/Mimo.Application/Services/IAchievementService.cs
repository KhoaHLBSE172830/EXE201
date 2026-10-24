using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Gamification;

namespace Mimo.Application.Services
{
    public interface IAchievementService
    {
        Task<BadgesResponseDto> GetAchievementsAsync(Guid userId);
        Task<List<BadgeDto>> CheckAndUnlockBadgesAsync(Guid userId);
    }
}
