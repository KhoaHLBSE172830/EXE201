using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Entities;

namespace Mimo.Application.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IGenericRepository<Badge> _badgeRepository;
        private readonly IGenericRepository<UserBadge> _userBadgeRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<LessonSession> _sessionRepository;
        private readonly IGenericRepository<UserNodeProgress> _progressRepository;
        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(
            IGenericRepository<Badge> badgeRepository,
            IGenericRepository<UserBadge> userBadgeRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<LessonSession> sessionRepository,
            IGenericRepository<UserNodeProgress> progressRepository,
            IGenericRepository<Node> nodeRepository,
            IUnitOfWork unitOfWork,
            ILogger<AchievementService> logger)
        {
            _badgeRepository = badgeRepository;
            _userBadgeRepository = userBadgeRepository;
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _sessionRepository = sessionRepository;
            _progressRepository = progressRepository;
            _nodeRepository = nodeRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BadgesResponseDto> GetAchievementsAsync(Guid userId)
        {
            // First check and unlock any newly eligible badges
            await CheckAndUnlockBadgesAsync(userId);

            var allBadges = await _badgeRepository.ListAsync();
            var userBadges = await _userBadgeRepository.WhereAsync(ub => ub.UserId == userId);

            var user = await _userRepository.FindByIdAsync(userId);
            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            var completedSessions = await _sessionRepository.WhereAsync(s => s.UserId == userId && s.Status == "Completed");
            var completedProgress = await _progressRepository.WhereAsync(p => p.UserId == userId && p.Status == "Completed");
            var completedNodeIds = completedProgress.Select(p => p.NodeId).ToHashSet();

            var nodesWithDomain = await _nodeRepository.ListAsync(nameof(Node.Unit));

            var badgeDtos = new List<BadgeDto>();
            foreach (var b in allBadges)
            {
                var unlocked = userBadges.FirstOrDefault(ub => ub.BadgeId == b.Id);
                int progress = 0;

                if (unlocked != null)
                {
                    progress = 100;
                }
                else
                {
                    switch (b.CriteriaType)
                    {
                        case "FirstLesson":
                            progress = completedSessions.Any() ? 100 : 0;
                            break;
                        case "StreakDays":
                            int streak = stats?.Streak ?? 0;
                            progress = Math.Min(100, (int)((double)streak / b.CriteriaValue * 100));
                            break;
                        case "PerfectLessons":
                            int perfectCount = completedSessions.Count(s => s.HeartsLost == 0);
                            progress = Math.Min(100, (int)((double)perfectCount / b.CriteriaValue * 100));
                            break;
                        case "TotalXp":
                            int totalXp = user?.TotalXp ?? 0;
                            progress = Math.Min(100, (int)((double)totalXp / b.CriteriaValue * 100));
                            break;
                        case "CompletedTopic":
                            Guid targetDomainId = b.CriteriaValue switch
                            {
                                1 => DomainConstants.FirstAidDomainId,
                                2 => DomainConstants.FireDomainId,
                                3 => DomainConstants.TrafficDomainId,
                                4 => DomainConstants.SchoolSafetyDomainId,
                                _ => Guid.Empty
                            };
                            var domainNodes = nodesWithDomain.Where(n => n.Unit != null && n.Unit.DomainId == targetDomainId).ToList();
                            if (domainNodes.Any())
                            {
                                int doneInDomain = domainNodes.Count(n => completedNodeIds.Contains(n.Id));
                                progress = Math.Min(100, (int)((double)doneInDomain / domainNodes.Count * 100));
                            }
                            break;
                    }
                }

                badgeDtos.Add(new BadgeDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Category = b.Category,
                    Description = b.Description,
                    Icon = b.Icon,
                    Rarity = b.Rarity,
                    CriteriaType = b.CriteriaType,
                    CriteriaValue = b.CriteriaValue,
                    ProgressPercent = progress,
                    IsUnlocked = unlocked != null,
                    UnlockedAt = unlocked?.UnlockedAt
                });
            }

            return new BadgesResponseDto
            {
                TotalBadges = badgeDtos.Count,
                UnlockedBadgesCount = badgeDtos.Count(b => b.IsUnlocked),
                Badges = badgeDtos
            };
        }

        public async Task<List<BadgeDto>> CheckAndUnlockBadgesAsync(Guid userId)
        {
            var newlyUnlocked = new List<BadgeDto>();

            var allBadges = await _badgeRepository.ListAsync();
            var existingUserBadges = await _userBadgeRepository.WhereAsync(ub => ub.UserId == userId);

            var user = await _userRepository.FindByIdAsync(userId);
            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            var completedSessions = await _sessionRepository.WhereAsync(s => s.UserId == userId && s.Status == "Completed");
            var completedProgress = await _progressRepository.WhereAsync(p => p.UserId == userId && p.Status == "Completed");
            var completedNodeIds = completedProgress.Select(p => p.NodeId).ToHashSet();

            var nodesWithDomain = await _nodeRepository.ListAsync(nameof(Node.Unit));

            bool hasChanges = false;
            foreach (var b in allBadges)
            {
                if (existingUserBadges.Any(ub => ub.BadgeId == b.Id))
                {
                    continue;
                }

                bool eligible = false;
                switch (b.CriteriaType)
                {
                    case "FirstLesson":
                        eligible = completedSessions.Any();
                        break;
                    case "StreakDays":
                        eligible = (stats?.Streak ?? 0) >= b.CriteriaValue;
                        break;
                    case "PerfectLessons":
                        int perfectCount = completedSessions.Count(s => s.HeartsLost == 0);
                        eligible = perfectCount >= b.CriteriaValue;
                        break;
                    case "TotalXp":
                        eligible = (user?.TotalXp ?? 0) >= b.CriteriaValue;
                        break;
                    case "CompletedTopic":
                        Guid targetDomainId = b.CriteriaValue switch
                        {
                            1 => DomainConstants.FirstAidDomainId,
                            2 => DomainConstants.FireDomainId,
                            3 => DomainConstants.TrafficDomainId,
                            4 => DomainConstants.SchoolSafetyDomainId,
                            _ => Guid.Empty
                        };
                        var domainNodes = nodesWithDomain.Where(n => n.Unit != null && n.Unit.DomainId == targetDomainId).ToList();
                        if (domainNodes.Any())
                        {
                            int doneInDomain = domainNodes.Count(n => completedNodeIds.Contains(n.Id));
                            eligible = doneInDomain >= domainNodes.Count;
                        }
                        break;
                }

                if (eligible)
                {
                    var newUb = new UserBadge
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        BadgeId = b.Id,
                        UnlockedAt = DateTime.UtcNow
                    };
                    await _userBadgeRepository.CreateAsync(newUb);
                    hasChanges = true;

                    newlyUnlocked.Add(new BadgeDto
                    {
                        Id = b.Id,
                        Title = b.Title,
                        Category = b.Category,
                        Description = b.Description,
                        Icon = b.Icon,
                        Rarity = b.Rarity,
                        CriteriaType = b.CriteriaType,
                        CriteriaValue = b.CriteriaValue,
                        ProgressPercent = 100,
                        IsUnlocked = true,
                        UnlockedAt = newUb.UnlockedAt
                    });
                }
            }

            if (hasChanges)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return newlyUnlocked;
        }
    }
}
