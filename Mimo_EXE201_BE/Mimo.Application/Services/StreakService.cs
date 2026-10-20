using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Streak;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class StreakService : IStreakService
    {
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly ILogger<StreakService> _logger;

        public StreakService(
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<User> userRepository,
            ILogger<StreakService> logger)
        {
            _statsRepository = statsRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<StreakResultDto> UpdateStreakAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

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
                stats = await _statsRepository.CreateAsync(stats);
            }

            // 1. Chuyển đổi sang múi giờ của người dùng (mặc định Asia/Ho_Chi_Minh) theo AGENT.md Section 5.2
            TimeZoneInfo userTimeZone;
            try
            {
                var tzName = string.IsNullOrWhiteSpace(user.Timezone) ? "Asia/Ho_Chi_Minh" : user.Timezone;
                userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzName);
            }
            catch
            {
                try
                {
                    // Fallback cho Windows
                    userTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch
                {
                    userTimeZone = TimeZoneInfo.Utc;
                }
            }

            var userLocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
            var currentDate = DateOnly.FromDateTime(userLocalTime);
            var lastActiveDate = stats.LastActiveDate;

            string status;
            bool freezeConsumed = false;

            // 2. Thuật toán phân nhánh chuỗi ngày (AGENT.md Section 5.2)
            if (!lastActiveDate.HasValue)
            {
                // TH3: Lần đầu tiên hoàn thành bài học
                stats.Streak = 1;
                stats.LastActiveDate = currentDate;
                status = "Started";
            }
            else if (lastActiveDate.Value == currentDate)
            {
                // TH1: Đã học bài trước đó trong cùng một ngày -> Giữ nguyên streak
                status = "Maintained";
            }
            else if (currentDate == lastActiveDate.Value.AddDays(1))
            {
                // TH2: Học liên tục ngày kế tiếp -> Tăng +1 ngày
                stats.Streak += 1;
                stats.LastActiveDate = currentDate;
                status = "Increased";
            }
            else
            {
                // TH4: Bị ngắt quãng từ 2 ngày trở lên
                int deltaDays = currentDate.DayNumber - lastActiveDate.Value.DayNumber - 1;

                if (stats.StreakFreezeCount >= deltaDays)
                {
                    // Tiêu thụ bùa Freeze để bảo toàn chuỗi
                    stats.StreakFreezeCount -= deltaDays;
                    stats.LastActiveDate = currentDate;
                    freezeConsumed = true;
                    status = "Frozen";
                    _logger.LogInformation("User {UserId} đã tiêu thụ {DeltaDays} bùa Freeze để bảo toàn Streak {Streak}",
                        userId, deltaDays, stats.Streak);
                }
                else if (deltaDays == 1 && stats.StreakFreezeCount >= 1)
                {
                    stats.StreakFreezeCount -= 1;
                    stats.LastActiveDate = currentDate;
                    freezeConsumed = true;
                    status = "Frozen";
                    _logger.LogInformation("User {UserId} đã tiêu thụ 1 bùa Freeze để bảo toàn Streak {Streak}",
                        userId, stats.Streak);
                }
                else
                {
                    // Không đủ bùa -> Chuỗi bị reset về 1
                    stats.Streak = 1;
                    stats.LastActiveDate = currentDate;
                    status = "Reset";
                    _logger.LogInformation("User {UserId} bị đứt chuỗi Streak, reset về 1", userId);
                }
            }

            await _statsRepository.UpdateAsync(stats);

            return new StreakResultDto
            {
                Streak = stats.Streak,
                Status = status,
                FreezeConsumed = freezeConsumed,
                FreezesRemaining = stats.StreakFreezeCount,
                LastActiveDate = lastActiveDate,
                CurrentDate = currentDate
            };
        }
    }
}
