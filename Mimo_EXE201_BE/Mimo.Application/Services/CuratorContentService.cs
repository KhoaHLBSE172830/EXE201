using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Curator;
using Mimo.Application.Helper;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class CuratorContentService : ICuratorContentService
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IGenericRepository<NodeQuestion> _nodeQuestionRepository;
        private readonly IGenericRepository<QuestionFeedback> _feedbackRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<SessionAnswer> _answerRepository;
        private readonly IQuestionGrader _questionGrader;
        private readonly IUnitOfWork _unitOfWork;

        public CuratorContentService(
            IGenericRepository<Question> questionRepository,
            IGenericRepository<Node> nodeRepository,
            IGenericRepository<NodeQuestion> nodeQuestionRepository,
            IGenericRepository<QuestionFeedback> feedbackRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<SessionAnswer> answerRepository,
            IQuestionGrader questionGrader,
            IUnitOfWork unitOfWork)
        {
            _questionRepository = questionRepository;
            _nodeRepository = nodeRepository;
            _nodeQuestionRepository = nodeQuestionRepository;
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _answerRepository = answerRepository;
            _questionGrader = questionGrader;
            _unitOfWork = unitOfWork;
        }

        private static void ValidateZeroLeakage(string payloadJson)
        {
            if (string.IsNullOrWhiteSpace(payloadJson))
            {
                throw new BadRequestException("Payload câu hỏi không được để trống.");
            }

            try
            {
                using var doc = JsonDocument.Parse(payloadJson);
                var raw = payloadJson.ToLowerInvariant();
                if (raw.Contains("\"iscorrect\"") || raw.Contains("\"correctoptionid\"") || raw.Contains("\"correctorder\""))
                {
                    throw new BadRequestException("Vi phạm nguyên tắc Zero Leakage: Payload hiển thị cho học sinh tuyệt đối không được chứa đáp án đúng (isCorrect, correctOptionId, correctOrder). Vui lòng tách riêng vào trường CorrectData.");
                }
            }
            catch (JsonException)
            {
                throw new BadRequestException("Payload câu hỏi phải là định dạng JSON hợp lệ.");
            }
        }

        private static void ValidateCorrectData(string type, string correctDataJson)
        {
            if (string.IsNullOrWhiteSpace(correctDataJson))
            {
                throw new BadRequestException("CorrectData không được để trống.");
            }

            try
            {
                using var doc = JsonDocument.Parse(correctDataJson);
            }
            catch (JsonException)
            {
                throw new BadRequestException("CorrectData phải là định dạng JSON hợp lệ.");
            }
        }

        public async Task<CuratorProfileDto> GetProfileAsync(Guid curatorId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == curatorId, "Profile");
            if (user == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin tác giả.");
            }

            var questions = await _questionRepository.WhereAsync(q => q.CuratorId == curatorId);

            int draft = questions.Count(q => string.Equals(q.Status, "Draft", StringComparison.OrdinalIgnoreCase));
            int pending = questions.Count(q => string.Equals(q.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase));
            int published = questions.Count(q => string.Equals(q.Status, "Published", StringComparison.OrdinalIgnoreCase));
            int rejected = questions.Count(q => string.Equals(q.Status, "Rejected", StringComparison.OrdinalIgnoreCase));

            return new CuratorProfileDto
            {
                CuratorId = user.Id,
                FullName = user.Profile?.FullName ?? user.Name,
                Email = user.Email,
                Specialty = user.Address ?? "Chuyên gia Kỹ năng sống & An toàn",
                DraftCount = draft,
                PendingCount = pending,
                PublishedCount = published,
                RejectedCount = rejected,
                TotalQuestions = questions.Count
            };
        }

        public async Task<List<CuratorQuestionDto>> GetQuestionsAsync(Guid curatorId, string? status = null, string? type = null, string? difficulty = null, CancellationToken cancellationToken = default)
        {
            var questions = await _questionRepository.WhereAsync(q => q.CuratorId == curatorId);

            if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
            {
                questions = questions.Where(q => string.Equals(q.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(type) && type != "ALL")
            {
                questions = questions.Where(q => string.Equals(q.Type, type, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(difficulty) && difficulty != "ALL")
            {
                questions = questions.Where(q => string.Equals(q.Difficulty, difficulty, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return questions.OrderByDescending(q => q.CreatedAt).Select(q => MapToQuestionDto(q)).ToList();
        }

        public async Task<CuratorQuestionDto> GetQuestionByIdAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default)
        {
            var q = await _questionRepository.FirstOrDefaultAsync(x => x.Id == questionId && x.CuratorId == curatorId);
            if (q == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi hoặc bạn không có quyền truy cập.");
            }

            return MapToQuestionDto(q);
        }

        public async Task<CuratorQuestionDto> CreateQuestionAsync(Guid curatorId, CreateCuratorQuestionRequestDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Tiêu đề câu hỏi không được để trống.");
            }

            ValidateZeroLeakage(request.Payload);
            ValidateCorrectData(request.Type, request.CorrectData);

            var question = new Question
            {
                CuratorId = curatorId,
                Type = request.Type.ToLowerInvariant(),
                Title = request.Title.Trim(),
                ImageUrl = request.ImageUrl,
                Payload = request.Payload,
                CorrectData = request.CorrectData,
                Explanation = request.Explanation.Trim(),
                Difficulty = request.Difficulty ?? "Medium",
                Source = request.Source,
                ReviewedBy = request.ReviewedBy,
                Status = "Draft"
            };

            question = await _questionRepository.CreateAsync(question);
            return MapToQuestionDto(question);
        }

        public async Task<CuratorQuestionDto> UpdateQuestionAsync(Guid curatorId, Guid questionId, UpdateCuratorQuestionRequestDto request, CancellationToken cancellationToken = default)
        {
            var q = await _questionRepository.FirstOrDefaultAsync(x => x.Id == questionId && x.CuratorId == curatorId);
            if (q == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi.");
            }

            if (string.Equals(q.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Câu hỏi đã xuất bản không được sửa trực tiếp. Vui lòng tạo bản sao Draft mới.");
            }

            ValidateZeroLeakage(request.Payload);
            ValidateCorrectData(request.Type, request.CorrectData);

            q.Title = request.Title.Trim();
            q.Type = request.Type.ToLowerInvariant();
            q.ImageUrl = request.ImageUrl;
            q.Payload = request.Payload;
            q.CorrectData = request.CorrectData;
            q.Explanation = request.Explanation.Trim();
            q.Difficulty = request.Difficulty ?? "Medium";
            q.Source = request.Source;
            q.ReviewedBy = request.ReviewedBy;
            q.Status = "Draft"; // Chuyển về Draft khi chỉnh sửa

            await _questionRepository.UpdateAsync(q);
            return MapToQuestionDto(q);
        }

        public async Task DeleteQuestionAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default)
        {
            var q = await _questionRepository.FirstOrDefaultAsync(x => x.Id == questionId && x.CuratorId == curatorId);
            if (q == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi.");
            }

            if (string.Equals(q.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Không thể xóa câu hỏi đã xuất bản trong chương trình học.");
            }

            await _questionRepository.DeleteAsync(q);
        }

        public async Task SubmitQuestionForApprovalAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default)
        {
            var q = await _questionRepository.FirstOrDefaultAsync(x => x.Id == questionId && x.CuratorId == curatorId);
            if (q == null)
            {
                throw new NotFoundException("Không tìm thấy câu hỏi.");
            }

            if (string.IsNullOrWhiteSpace(q.Source) || string.IsNullOrWhiteSpace(q.ReviewedBy))
            {
                throw new BadRequestException("Câu hỏi cần có nguồn trích dẫn chuyên môn (Source) và tên chuyên gia thẩm định (ReviewedBy) trước khi gửi duyệt.");
            }

            q.Status = "PendingApproval";
            await _questionRepository.UpdateAsync(q);
        }

        public async Task<List<CuratorLessonNodeDto>> GetLessonsAsync(Guid curatorId, CancellationToken cancellationToken = default)
        {
            var nodes = await _nodeRepository.WhereAsync(n => n.CuratorId == curatorId || n.CuratorId == null, "Unit");
            var nodeIds = nodes.Select(n => n.Id).ToList();

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nodeIds.Contains(nq.NodeId), "Question");

            var result = new List<CuratorLessonNodeDto>();
            foreach (var n in nodes.OrderBy(n => n.Order))
            {
                var questionsInNode = nodeQuestions.Where(nq => nq.NodeId == n.Id).OrderBy(nq => nq.Order).Select(nq => MapToQuestionDto(nq.Question)).ToList();

                result.Add(new CuratorLessonNodeDto
                {
                    Id = n.Id,
                    UnitId = n.UnitId,
                    UnitTitle = n.Unit?.Title ?? "Chuyên đề",
                    Title = n.Title,
                    Type = n.Type,
                    Order = n.Order,
                    Status = n.Status,
                    XpReward = n.XpReward,
                    QuestionsCount = questionsInNode.Count,
                    CuratorId = n.CuratorId,
                    Questions = questionsInNode
                });
            }

            return result;
        }

        public async Task<CuratorLessonNodeDto> CreateLessonAsync(Guid curatorId, CreateCuratorLessonRequestDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new BadRequestException("Tiêu đề bài học không được để trống.");
            }

            var node = new Node
            {
                CuratorId = curatorId,
                UnitId = request.UnitId,
                Title = request.Title.Trim(),
                Type = "lesson",
                Order = request.Order,
                XpReward = request.XpReward > 0 ? request.XpReward : 10,
                Status = "Draft"
            };

            node = await _nodeRepository.CreateAsync(node);

            return new CuratorLessonNodeDto
            {
                Id = node.Id,
                UnitId = node.UnitId,
                Title = node.Title,
                Type = node.Type,
                Order = node.Order,
                Status = node.Status,
                XpReward = node.XpReward,
                QuestionsCount = 0,
                CuratorId = curatorId
            };
        }

        public async Task<CuratorLessonNodeDto> UpdateLessonQuestionsAsync(Guid curatorId, Guid nodeId, UpdateLessonQuestionsRequestDto request, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId && (n.CuratorId == curatorId || n.CuratorId == null));
            if (node == null)
            {
                throw new NotFoundException("Không tìm thấy bài học.");
            }

            if (string.Equals(node.Status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Không thể sửa bài học đã xuất bản.");
            }

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Xóa toàn bộ liên kết câu hỏi cũ
            var oldLinks = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId);
            foreach (var link in oldLinks)
            {
                await _nodeQuestionRepository.DeleteAsync(link);
            }

            // Gắn danh sách câu hỏi mới theo thứ tự
            int order = 1;
            foreach (var qId in request.QuestionIds)
            {
                await _nodeQuestionRepository.CreateAsync(new NodeQuestion
                {
                    NodeId = nodeId,
                    QuestionId = qId,
                    Order = order++
                });
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return (await GetLessonsAsync(curatorId, cancellationToken)).First(l => l.Id == nodeId);
        }

        public async Task SubmitLessonForApprovalAsync(Guid curatorId, Guid nodeId, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId && (n.CuratorId == curatorId || n.CuratorId == null));
            if (node == null)
            {
                throw new NotFoundException("Không tìm thấy bài học.");
            }

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId, "Question");
            if (nodeQuestions.Count < 10)
            {
                throw new BadRequestException($"Một bài học chuẩn yêu cầu tối thiểu 10 câu hỏi để đảm bảo trải nghiệm học tập đầy đủ (hiện có: {nodeQuestions.Count} câu).");
            }

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            node.Status = "PendingApproval";
            await _nodeRepository.UpdateAsync(node);

            foreach (var nq in nodeQuestions)
            {
                if (nq.Question != null && nq.Question.Status == "Draft")
                {
                    nq.Question.Status = "PendingApproval";
                    await _questionRepository.UpdateAsync(nq.Question);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        public Task<CuratorPreviewAnswerResponseDto> PreviewAnswerAsync(CuratorPreviewAnswerRequestDto request, CancellationToken cancellationToken = default)
        {
            bool isCorrect = _questionGrader.Grade(request.QuestionType, request.SubmittedAnswerJson, request.CorrectDataJson);
            return Task.FromResult(new CuratorPreviewAnswerResponseDto { IsCorrect = isCorrect });
        }

        public async Task<List<CuratorAnalyticsItemDto>> GetAnalyticsAsync(Guid curatorId, CancellationToken cancellationToken = default)
        {
            var myQuestions = await _questionRepository.WhereAsync(q => q.CuratorId == curatorId);
            var qIds = myQuestions.Select(q => q.Id).ToList();

            var answers = await _answerRepository.WhereAsync(a => qIds.Contains(a.QuestionId));

            var result = new List<CuratorAnalyticsItemDto>();
            foreach (var q in myQuestions)
            {
                var qAnswers = answers.Where(a => a.QuestionId == q.Id).ToList();
                int total = qAnswers.Count;
                int wrong = qAnswers.Count(a => !a.IsCorrect);
                double rate = total > 0 ? Math.Round((double)wrong * 100 / total, 1) : 0;

                result.Add(new CuratorAnalyticsItemDto
                {
                    QuestionId = q.Id,
                    Title = q.Title,
                    Type = q.Type,
                    TotalAttempts = total,
                    WrongAttempts = wrong,
                    WrongRate = rate
                });
            }

            return result;
        }

        public async Task<List<CuratorFeedbackDto>> GetFeedbacksAsync(Guid curatorId, CancellationToken cancellationToken = default)
        {
            var myQuestions = await _questionRepository.WhereAsync(q => q.CuratorId == curatorId);
            var qIds = myQuestions.Select(q => q.Id).ToList();

            var feedbacks = await _feedbackRepository.WhereAsync(f => qIds.Contains(f.QuestionId), "Question");

            return feedbacks.OrderByDescending(f => f.CreatedAt).Select(f => new CuratorFeedbackDto
            {
                Id = f.Id,
                QuestionId = f.QuestionId,
                QuestionTitle = f.Question?.Title ?? "Câu hỏi",
                ReviewerName = f.ReviewerName,
                Kind = f.Kind,
                Note = f.Note,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                ResolvedAt = f.ResolvedAt
            }).ToList();
        }

        public async Task ResolveFeedbackAsync(Guid curatorId, Guid feedbackId, ResolveFeedbackRequestDto request, CancellationToken cancellationToken = default)
        {
            var feedback = await _feedbackRepository.FirstOrDefaultAsync(f => f.Id == feedbackId);
            if (feedback == null)
            {
                throw new NotFoundException("Không tìm thấy phản hồi.");
            }

            feedback.Status = "Resolved";
            feedback.ResolvedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.ResolutionNote))
            {
                feedback.Note += $"\n[Đã xử lý bởi Curator lúc {DateTime.UtcNow:dd/MM/yyyy HH:mm}]: {request.ResolutionNote}";
            }

            await _feedbackRepository.UpdateAsync(feedback);
        }

        private static CuratorQuestionDto MapToQuestionDto(Question q)
        {
            object? payload = null;
            object? correct = null;
            try { payload = JsonSerializer.Deserialize<object>(q.Payload); } catch { payload = q.Payload; }
            try { correct = JsonSerializer.Deserialize<object>(q.CorrectData); } catch { correct = q.CorrectData; }

            return new CuratorQuestionDto
            {
                Id = q.Id,
                Type = q.Type,
                Title = q.Title,
                ImageUrl = q.ImageUrl,
                Payload = payload,
                CorrectData = correct,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Source = q.Source,
                ReviewedBy = q.ReviewedBy,
                Status = q.Status,
                CuratorId = q.CuratorId,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            };
        }
    }
}
