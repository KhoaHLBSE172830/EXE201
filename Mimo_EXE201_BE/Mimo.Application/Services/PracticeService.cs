using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mimo.Application.DTOs.Practice;
using Mimo.Application.Helper;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class PracticeService : IPracticeService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<UserStats> _statsRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<WrongQuestion> _wrongQuestionRepository;
        private readonly IGenericRepository<XpEvent> _xpEventRepository;
        private readonly IQuestionGrader _questionGrader;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PracticeService> _logger;

        public PracticeService(
            IGenericRepository<User> userRepository,
            IGenericRepository<UserStats> statsRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<WrongQuestion> wrongQuestionRepository,
            IGenericRepository<XpEvent> xpEventRepository,
            IQuestionGrader questionGrader,
            IUnitOfWork unitOfWork,
            ILogger<PracticeService> logger)
        {
            _userRepository = userRepository;
            _statsRepository = statsRepository;
            _questionRepository = questionRepository;
            _wrongQuestionRepository = wrongQuestionRepository;
            _xpEventRepository = xpEventRepository;
            _questionGrader = questionGrader;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<WrongQuestionDto>> GetWrongQuestionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var wrongQuestions = await _wrongQuestionRepository.WhereAsync(
                w => w.UserId == userId && w.ResolvedAt == null,
                "Question");

            var result = new List<WrongQuestionDto>();

            foreach (var wq in wrongQuestions.OrderByDescending(w => w.LastAttemptAt))
            {
                var q = wq.Question;
                if (q == null) continue;

                object? payloadObj = null;
                try
                {
                    payloadObj = JsonSerializer.Deserialize<object>(q.Payload);
                }
                catch
                {
                    payloadObj = q.Payload;
                }

                // ZERO LEAKAGE: Không serialize CorrectData
                result.Add(new WrongQuestionDto
                {
                    Id = wq.Id,
                    QuestionId = q.Id,
                    Type = q.Type,
                    Title = q.Title,
                    ImageUrl = q.ImageUrl,
                    Payload = payloadObj,
                    TimesWrong = wq.TimesWrong,
                    LastAttemptAt = wq.LastAttemptAt
                });
            }

            return result;
        }

        public async Task<PracticeAnswerResponseDto> SubmitPracticeAnswerAsync(
            Guid userId,
            PracticeAnswerRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var question = await _questionRepository.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
            if (question == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi.");
            }

            // BUG-03 FIX: Chặn farm tim/XP vô hạn - Bắt buộc câu hỏi phải nằm trong WrongQuestions và chưa được giải quyết
            var wrongQuestion = await _wrongQuestionRepository.FirstOrDefaultAsync(
                w => w.UserId == userId && w.QuestionId == request.QuestionId && w.ResolvedAt == null);

            if (wrongQuestion == null)
            {
                throw new BadRequestException("Câu hỏi này không nằm trong danh sách câu sai cần ôn tập hoặc đã được hoàn thành trước đó.");
            }

            var stats = await _statsRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (stats == null)
            {
                stats = new UserStats
                {
                    UserId = userId,
                    Hearts = 5,
                    HeartsUpdatedAt = DateTime.UtcNow,
                    Streak = 0
                };
                stats = await _statsRepository.CreateAsync(stats);
            }

            var rawSubmittedJson = request.SubmittedAnswer.GetRawText();
            bool isCorrect = _questionGrader.Grade(question.Type, rawSubmittedJson, question.CorrectData);

            int heartRecovered = 0;
            int xpEarned = 0;

            // BUG-06 FIX: Giao dịch đảm bảo tính toàn vẹn dữ liệu
            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            if (isCorrect)
            {
                // 1. Đánh dấu ResolvedAt trong WrongQuestions
                wrongQuestion.ResolvedAt = DateTime.UtcNow;
                await _wrongQuestionRepository.UpdateAsync(wrongQuestion);

                // 2. Hồi phục +1 Tim (tối đa 5 tim) theo AGENT.md Section 5.1
                if (stats.Hearts < 5 && !stats.IsPro)
                {
                    stats.Hearts = Math.Min(5, stats.Hearts + 1);
                    // BUG-08 FIX: Chỉ reset HeartsUpdatedAt khi đã full 5 tim.
                    // Nếu chưa full 5 tim, giữ nguyên mốc để bảo toàn thời gian người dùng đã chờ tự hồi tim tự nhiên!
                    if (stats.Hearts >= 5)
                    {
                        stats.HeartsUpdatedAt = DateTime.UtcNow;
                    }
                    await _statsRepository.UpdateAsync(stats);
                    heartRecovered = 1;
                }

                // 3. Thưởng +5 XP ghi vào Sổ cái XP Ledger (source: practice_reward)
                xpEarned = 5;
                await _xpEventRepository.CreateAsync(new XpEvent
                {
                    UserId = userId,
                    Amount = xpEarned,
                    Source = "practice_reward",
                    ReferenceId = question.Id,
                    CreatedAt = DateTime.UtcNow
                });

                var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.TotalXp += xpEarned;
                    await _userRepository.UpdateAsync(user);
                }

                _logger.LogInformation("User {UserId} ôn tập đúng câu {QuestionId}: Hồi +{Heart} Tim, Thưởng +{Xp} XP",
                    userId, question.Id, heartRecovered, xpEarned);
            }
            else
            {
                // Nếu sai: Không trừ tim trong phòng ôn tập, cập nhật số lần sai
                wrongQuestion.TimesWrong += 1;
                wrongQuestion.LastAttemptAt = DateTime.UtcNow;
                await _wrongQuestionRepository.UpdateAsync(wrongQuestion);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new PracticeAnswerResponseDto
            {
                QuestionId = question.Id,
                IsCorrect = isCorrect,
                Explanation = question.Explanation,
                HeartsRemaining = stats.Hearts,
                HeartRecovered = heartRecovered,
                XpEarned = xpEarned
            };
        }
    }
}
