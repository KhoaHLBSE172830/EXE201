using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.User;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;
using Mimo.DomainLayer.Helper;

namespace Mimo.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserProfile> _profileRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly ICryptoService _cryptoService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IGenericRepository<User> userRepository,
            IGenericRepository<UserProfile> profileRepository,
            IGenericRepository<UserStats> statsRepository,
            ICryptoService cryptoService,
            ICacheService cacheService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _statsRepository = statsRepository;
            _cryptoService = cryptoService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<UserDetailDto> GetMeAsync(Guid userId)
        {
            var user = await _userRepository.FirstOrDefaultAsync(
                u => u.Id == userId,
                nameof(User.Role),
                nameof(User.Organization),
                nameof(User.Profile),
                nameof(User.Stats));

            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            // Đảm bảo UserStats luôn tồn tại (phòng trường hợp tài khoản cũ chưa có)
            if (user.Stats == null)
            {
                user.Stats = new UserStats
                {
                    UserId = user.Id,
                    Hearts = 5,
                    HeartsUpdatedAt = DateTime.UtcNow,
                    Streak = 0,
                    StreakFreezeCount = 0,
                    IsPro = false
                };
                await _statsRepository.CreateAsync(user.Stats);
            }

            // Đảm bảo UserProfile luôn tồn tại
            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    UserId = user.Id,
                    FullName = user.Name,
                    AvatarUrl = user.Avatar,
                    DailyGoalMinutes = 10
                };
                await _profileRepository.CreateAsync(user.Profile);
            }

            // Áp dụng thuật toán Lazy Heart Regeneration theo AGENT.md Section 5.1
            var (currentHearts, nextHeartInSeconds, updated) = user.Stats.CalculateCurrentHearts(DateTime.UtcNow);
            if (updated)
            {
                await _statsRepository.UpdateAsync(user.Stats);
            }

            // Tính toán Level theo AGENT.md Section 5.3
            var levelInfo = LevelHelper.GetLevelInfo(user.TotalXp);

            return new UserDetailDto
            {
                Id = user.Id,
                FullName = user.Profile?.FullName ?? user.Name,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.Phone,
                Address = user.Address,
                AvatarUrl = user.Profile?.AvatarUrl ?? user.Avatar,
                Status = user.Status,
                AuthType = user.AuthType ?? "Local",
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Student",
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization?.Name,
                Timezone = user.Timezone,

                // XP & Level
                TotalXp = user.TotalXp,
                Level = levelInfo.Level,
                LevelTitle = levelInfo.Title,
                XpToNextLevel = levelInfo.XpToNextLevel,
                LevelProgressPercent = levelInfo.ProgressPercent,

                // Hearts
                Hearts = currentHearts,
                MaxHearts = 5,
                NextHeartInSeconds = nextHeartInSeconds,
                IsPro = user.Stats.IsPro,
                ProExpiresAt = user.Stats.ProExpiresAt,

                // Streak
                Streak = user.Stats.Streak,
                StreakFreezeCount = user.Stats.StreakFreezeCount,
                LastActiveDate = user.Stats.LastActiveDate,

                // Onboarding
                DailyGoalMinutes = user.Profile?.DailyGoalMinutes ?? 10,
                RiskSurveyResult = user.Profile?.RiskSurveyResult,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDetailDto> UpdateOnboardingAsync(Guid userId, UpdateOnboardingDto request)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId, nameof(User.Profile));
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            if (user.Profile == null)
            {
                user.Profile = new UserProfile
                {
                    UserId = user.Id,
                    FullName = user.Name,
                    DailyGoalMinutes = request.DailyGoalMinutes,
                    RiskSurveyResult = request.RiskSurveyResult
                };
                await _profileRepository.CreateAsync(user.Profile);
            }
            else
            {
                user.Profile.DailyGoalMinutes = request.DailyGoalMinutes;
                if (!string.IsNullOrWhiteSpace(request.RiskSurveyResult))
                {
                    user.Profile.RiskSurveyResult = request.RiskSurveyResult;
                }
                user.Profile.UpdatedAt = DateTime.UtcNow;
                await _profileRepository.UpdateAsync(user.Profile);
            }

            return await GetMeAsync(userId);
        }

        public async Task<UserDetailDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId, nameof(User.Profile));
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            user.Name = request.FullName.Trim();
            user.Phone = request.PhoneNumber;
            user.Address = request.Address;
            user.Avatar = request.AvatarUrl;
            if (!string.IsNullOrWhiteSpace(request.Timezone))
            {
                user.Timezone = request.Timezone.Trim();
            }
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);


            if (user.Profile != null)
            {
                user.Profile.FullName = user.Name;
                user.Profile.AvatarUrl = user.Avatar;
                user.Profile.UpdatedAt = DateTime.UtcNow;
                await _profileRepository.UpdateAsync(user.Profile);
            }

            return await GetMeAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin người dùng.");
            }

            // 1. Xác thực mật khẩu cũ
            if (!_cryptoService.VerifyPassword(request.CurrentPassword, user.Password))
            {
                throw new BadRequestException("Mật khẩu hiện tại không chính xác.");
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                throw new BadRequestException("Mật khẩu mới không được trùng với mật khẩu cũ.");
            }

            // 2. Băm và cập nhật mật khẩu mới
            user.Password = _cryptoService.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            // Xoay vòng Refresh Token
            user.RefreshToken = _cryptoService.GenerateRandomToken(32);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddSeconds(JwtConst.REFRESH_TOKEN_EXP);
            await _userRepository.UpdateAsync(user);

            // 3. Đăng xuất khỏi toàn bộ các thiết bị/phiên cũ trong Redis
            try
            {
                await _cacheService.ForceLogout(userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi ForceLogout phiên cũ sau đổi mật khẩu: {Message}", ex.Message);
            }

            return true;
        }
    }
}
