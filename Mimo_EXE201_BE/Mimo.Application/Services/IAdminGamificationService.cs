using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminGamificationService
    {
        Task<GamificationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
        Task<GamificationSettingsDto> SaveSettingsAsync(GamificationSettingsDto settings, CancellationToken cancellationToken = default);
        Task<GamificationSettingsDto> ResetSettingsAsync(CancellationToken cancellationToken = default);
    }
}
