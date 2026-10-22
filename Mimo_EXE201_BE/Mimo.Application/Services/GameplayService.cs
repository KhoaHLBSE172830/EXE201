using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Gameplay;
using Mimo.Application.Helper;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class GameplayService : IGameplayService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IGenericRepository<NodeQuestion> _nodeQuestionRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<LessonSession> _sessionRepository;
        private readonly IGenericRepository<SessionAnswer> _sessionAnswerRepository;
        private readonly IGenericRepository<UserNodeProgress> _progressRepository;
        private readonly IGenericRepository<WrongQuestion> _wrongQuestionRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IQuestionGrader _questionGrader;
        private readonly IStreakService _streakService;
        private readonly ILogger<GameplayService> _logger;

        public GameplayService(
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<Node> nodeRepository,
            IGenericRepository<NodeQuestion> nodeQuestionRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<LessonSession> sessionRepository,
            IGenericRepository<SessionAnswer> sessionAnswerRepository,
            IGenericRepository<UserNodeProgress> progressRepository,
            IGenericRepository<WrongQuestion> wrongQuestionRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IQuestionGrader questionGrader,
            IStreakService streakService,
            ILogger<GameplayService> logger)
        {
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _nodeRepository = nodeRepository;
            _nodeQuestionRepository = nodeQuestionRepository;
            _questionRepository = questionRepository;
            _sessionRepository = sessionRepository;
            _sessionAnswerRepository = sessionAnswerRepository;
            _progressRepository = progressRepository;
            _wrongQuestionRepository = wrongQuestionRepository;
            _xpEventRepository = xpEventRepository;
            _questionGrader = questionGrader;
            _streakService = streakService;
            _logger = logger;
        }

        public async Task<StartLessonResponseDto> StartLessonAsync(Guid userId, Guid nodeId, CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra tim và Lazy Heart Regeneration theo AGENT.md Section 5.1
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
            else
            {
                var (_, _, updated) = stats.CalculateCurrentHearts(DateTime.UtcNow);
                if (updated)
                {
                    await _statsRepository.UpdateAsync(stats);
                }
            }

            if (stats.Hearts <= 0 && !stats.IsPro)
            {
                throw new BadRequestException("Bạn đã hết tim. Vui lòng luyện tập tại phòng ôn tập để hồi tim hoặc đợi hồi tim tự động.");
            }

            // 2. Kiểm tra node tồn tại và trạng thái Published (AGENT.md Section 5.5 Gatekeeper)
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null || !string.Equals(node.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotFoundException("Bài học không tồn tại hoặc chưa được xuất bản.");
            }

            // 3. Lấy danh sách câu hỏi trong node qua NodeQuestions
            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId, "Question");
            var orderedNodeQuestions = nodeQuestions.OrderBy(nq => nq.Order).ToList();

            if (orderedNodeQuestions.Count == 0)
            {
                throw new BadRequestException("Bài học này chưa có câu hỏi nào được gán.");
            }

            // 4. Khởi tạo phiên học LessonSession (TTL 30 phút, AGENT.md Section 4.3)
            var startedAt = DateTime.UtcNow;
            var session = new LessonSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                NodeId = nodeId,
                Status = "in_progress",
                HeartsLost = 0,
                TotalXpEarned = 0,
                CurrentComboStreak = 0,
                MaxComboStreak = 0,
                StartedAt = startedAt,
                ExpiresAt = startedAt.AddMinutes(30)
            };
            session = await _sessionRepository.CreateAsync(session);

            // 5. Cập nhật UserNodeProgress sang 'current' nếu chưa hoàn thành
            var progress = await _progressRepository.FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == nodeId);
            if (progress == null)
            {
                await _progressRepository.CreateAsync(new UserNodeProgress
                {
                    UserId = userId,
                    NodeId = nodeId,
                    Status = "current"
                });
            }
            else if (string.Equals(progress.Status, "locked", StringComparison.OrdinalIgnoreCase))
            {
                progress.Status = "current";
                await _progressRepository.UpdateAsync(progress);
            }

            // 6. Serialize danh sách câu hỏi hiển thị (ZERO LEAKAGE: TUYỆT ĐỐI không chứa CorrectData)
            var questionDtos = new List<QuestionDisplayDto>();
            foreach (var nq in orderedNodeQuestions)
            {
                var q = nq.Question;
                object? payloadObj = null;
                try
                {
                    payloadObj = JsonSerializer.Deserialize<object>(q.Payload);
                }
                catch
                {
                    payloadObj = q.Payload;
                }

                questionDtos.Add(new QuestionDisplayDto
                {
                    Id = q.Id,
                    Type = q.Type,
                    Title = q.Title,
                    ImageUrl = q.ImageUrl,
                    Payload = payloadObj,
                    Difficulty = q.Difficulty,
                    Source = q.Source,
                    Order = nq.Order
                });
            }

            return new StartLessonResponseDto
            {
                SessionId = session.Id,
                NodeId = nodeId,
                NodeTitle = node.Title,
                StartedAt = session.StartedAt,
                ExpiresAt = session.ExpiresAt,
                HeartsRemaining = stats.Hearts,
                IsPro = stats.IsPro,
                Questions = questionDtos
            };
        }

        public async Task<SubmitAnswerResponseDto> SubmitAnswerAsync(
            Guid userId,
            Guid sessionId,
            SubmitAnswerRequestDto request,
            string? idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            // 1. Kiểm tra session
            var session = await _sessionRepository.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null || session.UserId != userId)
            {
                throw new NotFoundException("Không tìm thấy phiên học.");
            }

            if (!string.Equals(session.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Phiên học đã kết thúc hoặc không còn khả dụng.");
            }

            if (DateTime.UtcNow > session.ExpiresAt)
            {
                session.Status = "failed";
                await _sessionRepository.UpdateAsync(session);
                throw new BadRequestException("Phiên học đã hết hạn 30 phút.");
            }

            // 2. Kiểm tra Idempotency (Chống nộp 2 lần trừ 2 tim / cộng 2 lần XP)
            var existingAnswer = await _sessionAnswerRepository.FirstOrDefaultAsync(sa =>
                sa.SessionId == sessionId && (sa.QuestionId == request.QuestionId ||
                (!string.IsNullOrEmpty(idempotencyKey) && sa.IdempotencyKey == idempotencyKey)));

            var question = await _questionRepository.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
            if (question == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi.");
            }

            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            int currentHearts = stats?.Hearts ?? 0;

            if (existingAnswer != null)
            {
                _logger.LogInformation("Idempotent replay câu hỏi {QuestionId} cho session {SessionId}", request.QuestionId, sessionId);
                return new SubmitAnswerResponseDto
                {
                    QuestionId = existingAnswer.QuestionId,
                    IsCorrect = existingAnswer.IsCorrect,
                    Explanation = question.Explanation,
                    HeartsRemaining = currentHearts,
                    CurrentComboStreak = session.CurrentComboStreak,
                    XpEarned = existingAnswer.XpEarned,
                    ComboBonusXp = 0,
                    IsSessionFailed = currentHearts <= 0 && !(stats?.IsPro ?? false),
                    IdempotentReplay = true
                };
            }

            // 3. Engine chấm điểm độc lập (Choice, Sequence, Red Flag)
            var rawSubmittedJson = request.SubmittedAnswer.GetRawText();
            bool isCorrect = _questionGrader.Grade(question.Type, rawSubmittedJson, question.CorrectData);

            int xpEarned = 0;
            int comboBonus = 0;

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);

            if (isCorrect)
            {
                // Đúng: Tăng combo
                session.CurrentComboStreak += 1;
                session.MaxComboStreak = Math.Max(session.MaxComboStreak, session.CurrentComboStreak);

                // Thưởng Combo theo AGENT.md Section 5.3:
                // Combo 3: +2 XP, Combo 5: +3 XP, Combo 7: +4 XP, Combo 10: +5 XP
                comboBonus = session.CurrentComboStreak switch
                {
                    3 => 2,
                    5 => 3,
                    7 => 4,
                    10 => 5,
                    _ => 0
                };

                const int baseScore = 10;
                xpEarned = baseScore + comboBonus;

                session.TotalXpEarned += xpEarned;

                // Ghi nhận vào Ledger XP bất biến (xp_events)
                await _xpEventRepository.CreateAsync(new XpEvent
                {
                    UserId = userId,
                    Amount = baseScore,
                    Source = "lesson_correct",
                    ReferenceId = session.Id,
                    CreatedAt = DateTime.UtcNow
                });

                if (comboBonus > 0)
                {
                    await _xpEventRepository.CreateAsync(new XpEvent
                    {
                        UserId = userId,
                        Amount = comboBonus,
                        Source = "combo_bonus",
                        ReferenceId = session.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                // Cập nhật snapshot TotalXp
                if (user != null)
                {
                    user.TotalXp += xpEarned;
                    await _userRepository.UpdateAsync(user);
                }
            }
            else
            {
                // Sai: Reset combo, trừ 1 Tim, ghi WrongQuestions
                session.CurrentComboStreak = 0;
                session.HeartsLost += 1;

                if (stats != null && !stats.IsPro)
                {
                    stats.Hearts = Math.Max(0, stats.Hearts - 1);
                    stats.HeartsUpdatedAt = DateTime.UtcNow;
                    await _statsRepository.UpdateAsync(stats);
                    currentHearts = stats.Hearts;
                }

                // Ghi vào WrongQuestions
                var existingWrong = await _wrongQuestionRepository.FirstOrDefaultAsync(w => w.UserId == userId && w.QuestionId == question.Id);
                if (existingWrong != null)
                {
                    existingWrong.TimesWrong += 1;
                    existingWrong.LastAttemptAt = DateTime.UtcNow;
                    existingWrong.ResolvedAt = null;
                    await _wrongQuestionRepository.UpdateAsync(existingWrong);
                }
                else
                {
                    await _wrongQuestionRepository.CreateAsync(new WrongQuestion
                    {
                        UserId = userId,
                        QuestionId = question.Id,
                        TimesWrong = 1,
                        LastAttemptAt = DateTime.UtcNow,
                        ResolvedAt = null
                    });
                }
            }

            // 4. Lưu bản ghi nộp câu trả lời SessionAnswer
            await _sessionAnswerRepository.CreateAsync(new SessionAnswer
            {
                SessionId = session.Id,
                QuestionId = question.Id,
                SubmittedAnswer = rawSubmittedJson,
                IsCorrect = isCorrect,
                TimeSpentSeconds = request.TimeSpentSeconds,
                XpEarned = xpEarned,
                IdempotencyKey = idempotencyKey
            });

            await _sessionRepository.UpdateAsync(session);

            bool isSessionFailed = currentHearts <= 0 && !(stats?.IsPro ?? false);

            return new SubmitAnswerResponseDto
            {
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                Explanation = question.Explanation,
                HeartsRemaining = currentHearts,
                CurrentComboStreak = session.CurrentComboStreak,
                XpEarned = xpEarned,
                ComboBonusXp = comboBonus,
                IsSessionFailed = isSessionFailed,
                IdempotentReplay = false
            };
        }

        public async Task<CompleteSessionResponseDto> CompleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await _sessionRepository.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null || session.UserId != userId)
            {
                throw new NotFoundException("Không tìm thấy phiên học.");
            }

            if (!string.Equals(session.Status, "in_progress", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Phiên học đã kết thúc trước đó.");
            }

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);

            // 1. Thưởng Perfect Lesson (+5 XP nếu không mất tim nào - AGENT.md Section 5.3)
            bool isPerfectLesson = session.HeartsLost == 0;
            int perfectBonusXp = 0;

            if (isPerfectLesson)
            {
                perfectBonusXp = 5;
                session.TotalXpEarned += perfectBonusXp;

                await _xpEventRepository.CreateAsync(new XpEvent
                {
                    UserId = userId,
                    Amount = perfectBonusXp,
                    Source = "perfect_lesson",
                    ReferenceId = session.Id,
                    CreatedAt = DateTime.UtcNow
                });

                if (user != null)
                {
                    user.TotalXp += perfectBonusXp;
                    await _userRepository.UpdateAsync(user);
                }
            }

            // 2. Chốt phiên học
            session.Status = "completed";
            session.CompletedAt = DateTime.UtcNow;
            await _sessionRepository.UpdateAsync(session);

            // 3. Cập nhật tiến độ node hiện tại sang 'done'
            var progress = await _progressRepository.FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == session.NodeId);
            if (progress != null)
            {
                progress.Status = "done";
                progress.CompletedAt = DateTime.UtcNow;
                await _progressRepository.UpdateAsync(progress);
            }
            else
            {
                await _progressRepository.CreateAsync(new UserNodeProgress
                {
                    UserId = userId,
                    NodeId = session.NodeId,
                    Status = "done",
                    CompletedAt = DateTime.UtcNow
                });
            }

            // 4. Tự động mở khóa node tiếp theo trong cùng Unit
            Guid? unlockedNextNodeId = null;
            var currentNode = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == session.NodeId);
            if (currentNode != null)
            {
                var nextNodes = await _nodeRepository.WhereAsync(n =>
                    n.UnitId == currentNode.UnitId &&
                    n.Order > currentNode.Order &&
                    n.Status == "Published");

                var nextNode = nextNodes.OrderBy(n => n.Order).FirstOrDefault();
                if (nextNode != null)
                {
                    unlockedNextNodeId = nextNode.Id;
                    var nextProgress = await _progressRepository.FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == nextNode.Id);
                    if (nextProgress == null)
                    {
                        await _progressRepository.CreateAsync(new UserNodeProgress
                        {
                            UserId = userId,
                            NodeId = nextNode.Id,
                            Status = "current"
                        });
                    }
                    else if (string.Equals(nextProgress.Status, "locked", StringComparison.OrdinalIgnoreCase))
                    {
                        nextProgress.Status = "current";
                        await _progressRepository.UpdateAsync(nextProgress);
                    }
                }
            }

            // 5. Cập nhật chuỗi Streak theo IStreakService
            var streakResult = await _streakService.UpdateStreakAsync(userId, cancellationToken);

            return new CompleteSessionResponseDto
            {
                SessionId = session.Id,
                NodeId = session.NodeId,
                Status = session.Status,
                TotalXpEarned = session.TotalXpEarned,
                HeartsLost = session.HeartsLost,
                HeartsRemaining = stats?.Hearts ?? 0,
                MaxComboStreak = session.MaxComboStreak,
                IsPerfectLesson = isPerfectLesson,
                PerfectBonusXp = perfectBonusXp,
                StreakInfo = streakResult,
                UnlockedNextNodeId = unlockedNextNodeId
            };
        }
    }
}
