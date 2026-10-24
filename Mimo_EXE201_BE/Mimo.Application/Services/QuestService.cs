using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class QuestService : IQuestService
    {
        private readonly IGenericRepository<DailyQuest> _questRepository;
        private readonly IGenericRepository<UserQuest> _userQuestRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IGenericRepository<LessonSession> _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QuestService> _logger;

        public QuestService(
            IGenericRepository<DailyQuest> questRepository,
            IGenericRepository<UserQuest> userQuestRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IGenericRepository<LessonSession> sessionRepository,
            IUnitOfWork unitOfWork,
            ILogger<QuestService> logger)
        {
            _questRepository = questRepository;
            _userQuestRepository = userQuestRepository;
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _xpEventRepository = xpEventRepository;
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DailyQuestsResponseDto> GetDailyQuestsAsync(Guid userId)
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var dateKey = DateOnly.FromDateTime(vnNow);
            var nextMidnightVn = vnNow.Date.AddDays(1);
            int resetInSeconds = (int)Math.Max(0, (nextMidnightVn - vnNow).TotalSeconds);

            var todayStartUtc = new DateTime(dateKey.Year, dateKey.Month, dateKey.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(-7);
            var todayEndUtc = todayStartUtc.AddDays(1);

            // 1. Get all active daily quests
            var activeQuests = await _questRepository.WhereAsync(q => q.IsActive);

            // 2. Get existing user quests for today
            var userQuests = await _userQuestRepository.WhereAsync(
                uq => uq.UserId == userId && uq.DateKey == dateKey,
                nameof(UserQuest.Quest));

            // Initialize missing user quests
            bool hasNew = false;
            foreach (var q in activeQuests)
            {
                if (!userQuests.Any(uq => uq.QuestId == q.Id))
                {
                    var newUq = new UserQuest
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        QuestId = q.Id,
                        Quest = q,
                        DateKey = dateKey,
                        CurrentProgress = 0,
                        IsCompleted = false,
                        IsClaimed = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _userQuestRepository.CreateAsync(newUq);
                    userQuests.Add(newUq);
                    hasNew = true;
                }
            }
            if (hasNew)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            // 3. Compute real-time daily progress
            var todayXpEvents = await _xpEventRepository.WhereAsync(
                x => x.UserId == userId && x.Amount > 0 && x.Source != "quest_reward" && x.CreatedAt >= todayStartUtc && x.CreatedAt < todayEndUtc);
            int todayXp = todayXpEvents.Sum(x => x.Amount);

            var todaySessions = await _sessionRepository.WhereAsync(
                s => s.UserId == userId && s.Status == "Completed" && s.CreatedAt >= todayStartUtc && s.CreatedAt < todayEndUtc);

            int completedLessons = todaySessions.Count;
            int perfectLessons = todaySessions.Count(s => s.HeartsLost == 0);

            bool needsUpdate = false;
            foreach (var uq in userQuests)
            {
                int calculated = uq.CurrentProgress;
                var qType = uq.Quest?.QuestType ?? string.Empty;

                switch (qType)
                {
                    case "XpGoal":
                        calculated = Math.Min(uq.Quest?.Target ?? 40, todayXp);
                        break;
                    case "LessonsCount":
                        calculated = Math.Min(uq.Quest?.Target ?? 2, completedLessons);
                        break;
                    case "PerfectLesson":
                        calculated = Math.Min(uq.Quest?.Target ?? 1, perfectLessons);
                        break;
                }

                if (calculated != uq.CurrentProgress)
                {
                    uq.CurrentProgress = calculated;
                    uq.IsCompleted = uq.CurrentProgress >= (uq.Quest?.Target ?? 0);
                    await _userQuestRepository.UpdateAsync(uq);
                    needsUpdate = true;
                }
                else if (uq.Quest != null && calculated >= uq.Quest.Target && !uq.IsCompleted)
                {
                    uq.IsCompleted = true;
                    await _userQuestRepository.UpdateAsync(uq);
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return new DailyQuestsResponseDto
            {
                DateKey = dateKey.ToString("yyyy-MM-dd"),
                ResetInSeconds = resetInSeconds,
                Quests = userQuests.Select(uq => new QuestDto
                {
                    Id = uq.QuestId,
                    Title = uq.Quest?.Title ?? string.Empty,
                    QuestType = uq.Quest?.QuestType ?? string.Empty,
                    Target = uq.Quest?.Target ?? 0,
                    CurrentProgress = uq.CurrentProgress,
                    RewardType = uq.Quest?.RewardType ?? "XP",
                    RewardAmount = uq.Quest?.RewardAmount ?? 0,
                    Icon = uq.Quest?.Icon ?? "🎯",
                    IsCompleted = uq.IsCompleted,
                    IsClaimed = uq.IsClaimed,
                    ClaimedAt = uq.ClaimedAt
                }).ToList()
            };
        }

        public async Task<ClaimQuestResponseDto> ClaimQuestRewardAsync(Guid userId, Guid questId)
        {
            var vnNow = DateTime.UtcNow.AddHours(7);
            var dateKey = DateOnly.FromDateTime(vnNow);

            var userQuest = await _userQuestRepository.FirstOrDefaultAsync(
                uq => uq.UserId == userId && uq.QuestId == questId && uq.DateKey == dateKey,
                nameof(UserQuest.Quest));

            if (userQuest == null)
            {
                throw new NotFoundException("Không tìm thấy nhiệm vụ hàng ngày của ngày hôm nay.");
            }

            if (!userQuest.IsCompleted)
            {
                throw new BadRequestException("Bạn chưa hoàn thành mục tiêu nhiệm vụ này.");
            }

            if (userQuest.IsClaimed)
            {
                throw new BadRequestException("Bạn đã nhận phần thưởng cho nhiệm vụ này rồi.");
            }

            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy người dùng.");
            }

            var userStats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (userStats == null)
            {
                userStats = new UserStats { UserId = userId, Hearts = 5, HeartsUpdatedAt = DateTime.UtcNow };
                await _statsRepository.CreateAsync(userStats);
            }

            userQuest.IsClaimed = true;
            userQuest.ClaimedAt = DateTime.UtcNow;
            await _userQuestRepository.UpdateAsync(userQuest);

            string rewardType = userQuest.Quest?.RewardType ?? "XP";
            int rewardAmount = userQuest.Quest?.RewardAmount ?? 10;
            string message;

            if (rewardType == "XP")
            {
                user.TotalXp += rewardAmount;
                await _userRepository.UpdateAsync(user);

                await _xpEventRepository.CreateAsync(new XpEvent
                {
                    UserId = userId,
                    Amount = rewardAmount,
                    Source = "quest_reward",
                    ReferenceId = questId,
                    CreatedAt = DateTime.UtcNow
                });
                message = $"Nhận thưởng thành công +{rewardAmount} XP!";
            }
            else if (rewardType == "Heart")
            {
                userStats.Hearts = Math.Min(5, userStats.Hearts + rewardAmount);
                userStats.HeartsUpdatedAt = DateTime.UtcNow;
                await _statsRepository.UpdateAsync(userStats);
                message = $"Nhận thưởng thành công +{rewardAmount} Trái Tim!";
            }
            else if (rewardType == "StreakFreeze")
            {
                userStats.StreakFreezeCount += rewardAmount;
                await _statsRepository.UpdateAsync(userStats);
                message = $"Nhận thưởng thành công +{rewardAmount} Bùa Đóng Băng Streak!";
            }
            else
            {
                message = "Nhận thưởng thành công!";
            }

            await _unitOfWork.SaveChangesAsync();

            return new ClaimQuestResponseDto
            {
                QuestId = questId,
                RewardType = rewardType,
                RewardAmount = rewardAmount,
                NewTotalXp = user.TotalXp,
                Hearts = userStats.Hearts,
                StreakFreezeCount = userStats.StreakFreezeCount,
                Message = message
            };
        }
    }
}
