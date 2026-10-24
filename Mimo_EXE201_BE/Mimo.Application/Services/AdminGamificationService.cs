using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public class AdminGamificationService : IAdminGamificationService
    {
        private const string CacheKey = "mimo:admin:gamification_settings";
        private static GamificationSettingsDto _fallbackSettings = new();

        private readonly ICacheService _cacheService;
        private readonly ILogger<AdminGamificationService> _logger;

        public AdminGamificationService(
            ICacheService cacheService,
            ILogger<AdminGamificationService> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<GamificationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var cached = await _cacheService.Get<GamificationSettingsDto>(CacheKey);
                if (cached != null)
                {
                    return cached;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể đọc Gamification Settings từ Redis cache, chuyển sang dùng bộ nhớ tạm: {Message}", ex.Message);
            }

            return _fallbackSettings;
        }

        public async Task<GamificationSettingsDto> SaveSettingsAsync(GamificationSettingsDto settings, CancellationToken cancellationToken = default)
        {
            // Validate boundaries
            settings.MaxHearts = Math.Clamp(settings.MaxHearts, 1, 10);
            settings.HeartRefillMinutes = Math.Clamp(settings.HeartRefillMinutes, 15, 1440);
            settings.PracticeQuestionsToRefill = Math.Clamp(settings.PracticeQuestionsToRefill, 1, 10);
            settings.HeartRefillPriceXP = Math.Max(settings.HeartRefillPriceXP, 10);

            settings.StreakMinLessonsDaily = Math.Clamp(settings.StreakMinLessonsDaily, 1, 5);
            settings.FreezePriceXP = Math.Max(settings.FreezePriceXP, 50);
            settings.MaxFreezeHolding = Math.Clamp(settings.MaxFreezeHolding, 1, 10);

            settings.CohortSize = Math.Clamp(settings.CohortSize, 10, 100);
            settings.PromotionCount = Math.Clamp(settings.PromotionCount, 1, settings.CohortSize / 2);
            settings.DemotionCount = Math.Clamp(settings.DemotionCount, 1, settings.CohortSize / 2);

            _fallbackSettings = settings;

            try
            {
                await _cacheService.Set(CacheKey, settings, TimeSpan.FromDays(30));
                _logger.LogInformation("Admin đã lưu Gamification Settings vào Redis cache thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chưa thể lưu Gamification Settings vào Redis, đã lưu vào bộ nhớ RAM tạm thời: {Message}", ex.Message);
            }

            return settings;
        }

        public async Task<GamificationSettingsDto> ResetSettingsAsync(CancellationToken cancellationToken = default)
        {
            var defaultSettings = new GamificationSettingsDto();
            _fallbackSettings = defaultSettings;

            try
            {
                await _cacheService.Set(CacheKey, defaultSettings, TimeSpan.FromDays(30));
                _logger.LogInformation("Admin đã khôi phục Gamification Settings về mặc định.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi cập nhật Redis lúc reset settings: {Message}", ex.Message);
            }

            return defaultSettings;
        }
    }
}
