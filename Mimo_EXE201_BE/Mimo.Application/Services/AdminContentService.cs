using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Curator;
using Mimo.Application.Repositories;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Application.Services
{
    public class AdminContentService : IAdminContentService
    {
        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<NodeQuestion> _nodeQuestionRepository;
        private readonly IGenericRepository<QuestionFeedback> _feedbackRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminContentService(
            IGenericRepository<Node> nodeRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<NodeQuestion> nodeQuestionRepository,
            IGenericRepository<QuestionFeedback> feedbackRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _nodeRepository = nodeRepository;
            _questionRepository = questionRepository;
            _nodeQuestionRepository = nodeQuestionRepository;
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CuratorLessonNodeDto>> GetPendingLessonsAsync(CancellationToken cancellationToken = default)
        {
            var nodes = await _nodeRepository.WhereAsync(n => n.Status == "PendingApproval", "Unit");
            var nodeIds = nodes.Select(n => n.Id).ToList();

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nodeIds.Contains(nq.NodeId), "Question");

            var result = new List<CuratorLessonNodeDto>();
            foreach (var n in nodes.OrderBy(n => n.CreatedAt))
            {
                var questions = nodeQuestions.Where(nq => nq.NodeId == n.Id).OrderBy(nq => nq.Order).Select(nq =>
                {
                    var q = nq.Question;
                    object? payload = null;
                    try { payload = JsonSerializer.Deserialize<object>(q.Payload); } catch { payload = q.Payload; }

                    return new CuratorQuestionDto
                    {
                        Id = q.Id,
                        Type = q.Type,
                        Title = q.Title,
                        ImageUrl = q.ImageUrl,
                        Payload = payload,
                        Explanation = q.Explanation,
                        Difficulty = q.Difficulty,
                        Source = q.Source,
                        ReviewedBy = q.ReviewedBy,
                        Status = q.Status,
                        CuratorId = q.CuratorId
                    };
                }).ToList();

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
                    QuestionsCount = questions.Count,
                    CuratorId = n.CuratorId,
                    Questions = questions
                });
            }

            return result;
        }

        public async Task PublishLessonAsync(Guid nodeId, Guid adminId, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null)
            {
                throw new NotFoundException("Không tìm thấy bài học.");
            }

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId, "Question");

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            node.Status = "Published";
            await _nodeRepository.UpdateAsync(node);

            foreach (var nq in nodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Published";
                    await _questionRepository.UpdateAsync(nq.Question);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        public async Task RejectLessonAsync(Guid nodeId, Guid adminId, string reason, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null)
            {
                throw new NotFoundException("Không tìm thấy bài học.");
            }

            var admin = await _userRepository.FirstOrDefaultAsync(u => u.Id == adminId);
            string adminName = admin?.Name ?? "Ban Quản Trị Hệ Thống";

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId, "Question");

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            node.Status = "Draft";
            await _nodeRepository.UpdateAsync(node);

            foreach (var nq in nodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Rejected";
                    await _questionRepository.UpdateAsync(nq.Question);

                    await _feedbackRepository.CreateAsync(new QuestionFeedback
                    {
                        QuestionId = nq.QuestionId,
                        ReviewerId = adminId,
                        ReviewerName = adminName,
                        Kind = "Reject",
                        Note = string.IsNullOrWhiteSpace(reason) ? "Nội dung cần chỉnh sửa bổ sung theo quy chuẩn sư phạm." : reason.Trim(),
                        Status = "Pending"
                    });
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }

        public async Task RollbackLessonAsync(Guid nodeId, Guid adminId, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FirstOrDefaultAsync(n => n.Id == nodeId);
            if (node == null)
            {
                throw new NotFoundException("Không tìm thấy bài học.");
            }

            var nodeQuestions = await _nodeQuestionRepository.WhereAsync(nq => nq.NodeId == nodeId, "Question");

            await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);

            node.Status = "Draft";
            await _nodeRepository.UpdateAsync(node);

            foreach (var nq in nodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Draft";
                    await _questionRepository.UpdateAsync(nq.Question);
                }
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
}
