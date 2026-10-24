using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services
{
    public class AdminUserService : IAdminUserService
    {
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IGenericRepository<LessonSession> _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IGenericRepository<LessonSession> sessionRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _xpEventRepository = xpEventRepository;
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AdminUserItemDto>> GetUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.WhereAsync(
                filter: null,
                orderBy: q => q.OrderByDescending(u => u.CreatedAt),
                page: null,
                pageSize: 100, // Top 100 users gần nhất
                nameof(User.Role),
                nameof(User.Profile),
                nameof(User.Stats),
                nameof(User.Organization)
            );

            var userIds = users.Select(u => u.Id).ToList();

            // Lấy các phiên học gần nhất để xây dựng User 360 History
            var recentSessions = await _sessionRepository.WhereAsync(
                s => userIds.Contains(s.UserId),
                nameof(LessonSession.Node)
            );

            var sessionsByUser = recentSessions
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.StartedAt).Take(5).ToList());

            var result = new List<AdminUserItemDto>();

            foreach (var u in users)
            {
                var stats = u.Stats;
                var roleName = u.Role?.RoleName?.ToUpperInvariant() ?? "LEARNER";

                var plan = "FREE";
                if (stats?.IsPro == true)
                {
                    plan = "PRO";
                }
                else if (u.OrganizationId.HasValue)
                {
                    plan = "SCHOOL_LICENSE";
                }

                var uiStatus = u.Status switch
                {
                    UserStatusEnum.Active => "ACTIVE",
                    UserStatusEnum.Disable => "BANNED",
                    _ => "DISABLED"
                };

                // Tính toán cờ LostStreakRecently dựa trên múi giờ của người dùng
                var lostStreakRecently = false;
                var streak = stats?.Streak ?? 0;

                if (stats != null)
                {
                    var tz = ResolveUserTimeZone(u.Timezone);
                    var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));

                    if (streak == 0)
                    {
                        lostStreakRecently = true;
                    }
                    else if (stats.LastActiveDate.HasValue)
                    {
                        var delta = today.DayNumber - stats.LastActiveDate.Value.DayNumber;
                        if (delta >= 2)
                        {
                            lostStreakRecently = true;
                        }
                    }
                }

                sessionsByUser.TryGetValue(u.Id, out var userSessions);
                var history = userSessions?.Select(s => new AdminUserHistoryDto
                {
                    Date = s.StartedAt.ToString("yyyy-MM-dd"),
                    Lesson = s.Node?.Title ?? "Bài tập tình huống",
                    Xp = s.TotalXpEarned,
                    Status = s.Status == "completed" ? "PASS" : "FAILED"
                }).ToList() ?? new List<AdminUserHistoryDto>();

                result.Add(new AdminUserItemDto
                {
                    Id = u.Id,
                    Name = u.Profile?.FullName ?? u.Name ?? "Người dùng Mimo",
                    Email = u.Email,
                    Avatar = u.Avatar ?? u.Profile?.AvatarUrl,
                    Streak = streak,
                    MaxStreak = Math.Max(streak, 10),
                    Hearts = stats?.Hearts ?? 5,
                    Xp = u.TotalXp,
                    Role = roleName,
                    Plan = plan,
                    SchoolName = u.Organization?.Name,
                    Status = uiStatus,
                    JoinedDate = u.CreatedAt?.ToString("yyyy-MM-dd") ?? "2026-09-01",
                    LostStreakRecently = lostStreakRecently,
                    ComplaintText = lostStreakRecently && streak == 0 ? "Học viên bị gián đoạn chuỗi ngày do sự cố khách quan." : null,
                    History = history
                });
            }

            return result;
        }

        public async Task<List<AdminUserItemDto>> RescueStreakAsync(Guid userId, int? targetStreak = null, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FindAsync(u => u.Id == userId, nameof(User.Stats));
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng có ID '{userId}'.");
            }

            var stats = user.Stats;
            if (stats == null)
            {
                stats = new UserStats
                {
                    UserId = userId,
                    Hearts = 5,
                    HeartsUpdatedAt = DateTime.UtcNow,
                    Streak = 0,
                    StreakFreezeCount = 1
                };
                stats = await _statsRepository.CreateAsync(stats);
            }

            // Khôi phục Streak
            int newStreak = targetStreak.HasValue && targetStreak.Value > 0
                ? targetStreak.Value
                : (stats.Streak <= 0 ? 5 : stats.Streak + 1);

            stats.Streak = newStreak;

            // Đồng bộ ngày học gần nhất về hôm nay theo múi giờ người dùng
            var tz = ResolveUserTimeZone(user.Timezone);
            var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
            stats.LastActiveDate = today;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin đã cứu chuỗi Streak cho User {UserId} ({Email}) thành {NewStreak} ngày", user.Id, user.Email, newStreak);

            return await GetUsersAsync(cancellationToken);
        }

        public async Task<List<AdminUserItemDto>> RefillHeartsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FindAsync(u => u.Id == userId, nameof(User.Stats));
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng có ID '{userId}'.");
            }

            var stats = user.Stats;
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
            else
            {
                stats.Hearts = 5;
                stats.HeartsUpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin CSKH đã bơm đầy 5 Trái Tim cho User {UserId} ({Email})", user.Id, user.Email);

            return await GetUsersAsync(cancellationToken);
        }

        public async Task<List<AdminUserItemDto>> RewardXpAsync(Guid userId, int amount, Guid adminUserId, CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
            {
                throw new BadRequestException("Số điểm XP thưởng CSKH phải lớn hơn 0.");
            }

            var user = await _userRepository.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng có ID '{userId}'.");
            }

            // ATTENTION 4: LEDGER-BASED XP SYSTEM (Sổ cái bất biến)
            var xpEvent = new XpEvent
            {
                UserId = userId,
                Amount = amount,
                Source = "admin_reward",
                ReferenceId = adminUserId != Guid.Empty ? adminUserId : null,
                CreatedAt = DateTime.UtcNow
            };

            await _xpEventRepository.CreateAsync(xpEvent);

            // Cập nhật snapshot TotalXp
            user.TotalXp += amount;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin CSKH đã thưởng +{Amount} XP cho User {UserId} ({Email}). Đã ghi nhận sổ cái XpEvent",
                amount, user.Id, user.Email);

            return await GetUsersAsync(cancellationToken);
        }

        public async Task<List<AdminUserItemDto>> ToggleBanAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FindAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy người dùng có ID '{userId}'.");
            }

            // BẢO VỆ SUPER ADMIN: Tuyệt đối không được ban tài khoản Super Admin
            if (user.RoleId == AdminRoleId)
            {
                throw new BadRequestException("BẢO VỆ HỆ THỐNG: Tuyệt đối không được khóa hoặc vô hiệu hóa tài khoản Super Admin.");
            }

            if (user.Status == UserStatusEnum.Active)
            {
                user.Status = UserStatusEnum.Disable;
                _logger.LogWarning("Admin đã KHÓA (Disable) tài khoản User {UserId} ({Email})", user.Id, user.Email);
            }
            else
            {
                user.Status = UserStatusEnum.Active;
                _logger.LogInformation("Admin đã MỞ KHÓA (Active) lại tài khoản User {UserId} ({Email})", user.Id, user.Email);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetUsersAsync(cancellationToken);
        }

        private static TimeZoneInfo ResolveUserTimeZone(string? timezone)
        {
            try
            {
                var tzName = string.IsNullOrWhiteSpace(timezone) ? "Asia/Ho_Chi_Minh" : timezone;
                return TimeZoneInfo.FindSystemTimeZoneById(tzName);
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }
        }
    }
}
