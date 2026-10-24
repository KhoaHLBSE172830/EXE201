using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Gamification;

namespace Mimo.Application.Services
{
    public interface IQuestService
    {
        Task<DailyQuestsResponseDto> GetDailyQuestsAsync(Guid userId);
        Task<ClaimQuestResponseDto> ClaimQuestRewardAsync(Guid userId, Guid questId);
    }
}
