using System;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Streak;

namespace Mimo.Application.Services
{
    public interface IStreakService
    {
        Task<StreakResultDto> UpdateStreakAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
