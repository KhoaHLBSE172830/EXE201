using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
    public class AdminContentService : IAdminContentService
    {
        private static readonly Guid CuratorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        private readonly IGenericRepository<Node> _nodeRepository;
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminContentService> _logger;

        public AdminContentService(
            IGenericRepository<Node> nodeRepository,
            IGenericRepository<Question> questionRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminContentService> logger)
        {
            _nodeRepository = nodeRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<AdminReviewItemDto>> GetReviewsAsync(string? status = null, CancellationToken cancellationToken = default)
        {
            var nodes = await _nodeRepository.WhereAsync(
                filter: null,
                orderBy: q => q.OrderByDescending(n => n.CreatedAt),
                page: null,
                pageSize: null,
                nameof(Node.Unit),
                $"{nameof(Node.Unit)}.{nameof(Unit.Domain)}",
                nameof(Node.NodeQuestions),
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}",
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}.{nameof(Question.Curator)}"
            );

            var items = nodes.Select(MapToReviewItemDto).ToList();

            if (!string.IsNullOrEmpty(status))
            {
                var normalizedStatus = status.Trim().ToUpperInvariant();
                items = items.Where(i => i.Status == normalizedStatus).ToList();
            }

            return items;
        }

        public async Task<AdminReviewItemDto> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FindAsync(
                n => n.Id == id,
                nameof(Node.Unit),
                $"{nameof(Node.Unit)}.{nameof(Unit.Domain)}",
                nameof(Node.NodeQuestions),
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}",
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}.{nameof(Question.Curator)}"
            );

            if (node == null)
            {
                throw new NotFoundException($"Không tìm thấy bài học có ID '{id}'.");
            }

            return MapToReviewItemDto(node);
        }

        public async Task<List<AdminReviewItemDto>> ApproveReviewAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FindAsync(
                n => n.Id == id,
                nameof(Node.NodeQuestions),
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}"
            );

            if (node == null)
            {
                throw new NotFoundException($"Không tìm thấy bài học có ID '{id}' để phê duyệt.");
            }

            // Gatekeeper approval: Chuyển trạng thái sang Published
            node.Status = "Published";
            node.UpdatedAt = DateTime.UtcNow;

            foreach (var nq in node.NodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Published";
                    nq.Question.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin đã phê duyệt xuất bản (Published) thành công Node {NodeId} - {NodeTitle}", node.Id, node.Title);

            return await GetReviewsAsync(null, cancellationToken);
        }

        public async Task<List<AdminReviewItemDto>> RejectReviewAsync(Guid id, string reason, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BadRequestException("Vui lòng cung cấp lý do từ chối để gửi phản hồi cho Curator.");
            }

            var node = await _nodeRepository.FindAsync(
                n => n.Id == id,
                nameof(Node.NodeQuestions),
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}"
            );

            if (node == null)
            {
                throw new NotFoundException($"Không tìm thấy bài học có ID '{id}' để từ chối.");
            }

            node.Status = "Rejected";
            node.UpdatedAt = DateTime.UtcNow;

            foreach (var nq in node.NodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Rejected";
                    nq.Question.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Admin đã từ chối (Rejected) Node {NodeId} - {NodeTitle}. Lý do: {Reason}", node.Id, node.Title, reason);

            return await GetReviewsAsync(null, cancellationToken);
        }

        public async Task<List<AdminReviewItemDto>> RollbackReviewAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var node = await _nodeRepository.FindAsync(
                n => n.Id == id,
                nameof(Node.NodeQuestions),
                $"{nameof(Node.NodeQuestions)}.{nameof(NodeQuestion.Question)}"
            );

            if (node == null)
            {
                throw new NotFoundException($"Không tìm thấy bài học có ID '{id}' để thu hồi.");
            }

            // Emergency Rollback: Chuyển khẩn cấp về Draft để ẩn khỏi học sinh ngay lập tức
            node.Status = "Draft";
            node.UpdatedAt = DateTime.UtcNow;

            foreach (var nq in node.NodeQuestions)
            {
                if (nq.Question != null)
                {
                    nq.Question.Status = "Draft";
                    nq.Question.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin đã thực hiện THU HỒI KHẨN CẤP (Emergency Rollback về Draft) đối với Node {NodeId} - {NodeTitle}", node.Id, node.Title);

            return await GetReviewsAsync(null, cancellationToken);
        }

        public async Task<List<AdminCuratorDto>> GetCuratorsAsync(CancellationToken cancellationToken = default)
        {
            var curators = await _userRepository.WhereAsync(
                u => u.RoleId == CuratorRoleId,
                nameof(User.Profile),
                nameof(User.Role)
            );

            var questions = await _questionRepository.WhereAsync(q => q.CuratorId != null);

            var questionsByCurator = questions.GroupBy(q => q.CuratorId!.Value).ToDictionary(
                g => g.Key,
                g => new
                {
                    Total = g.Count(),
                    Approved = g.Count(q => q.Status == "Published"),
                    Pending = g.Count(q => q.Status == "PendingApproval" || q.Status == "Draft")
                }
            );

            var result = new List<AdminCuratorDto>();

            foreach (var c in curators)
            {
                questionsByCurator.TryGetValue(c.Id, out var stats);

                result.Add(new AdminCuratorDto
                {
                    Id = c.Id,
                    Name = c.Profile?.FullName ?? c.Name ?? "Chuyên gia Mimo",
                    Email = c.Email,
                    Title = c.Role?.Description ?? "Chuyên gia Thẩm định Độc lập",
                    Organization = "Độc lập (Không thuộc trường)",
                    Avatar = c.Avatar ?? c.Profile?.AvatarUrl,
                    AssignedDomain = "Kỹ năng An toàn & Sơ cấp cứu",
                    TotalSubmitted = stats?.Total ?? 0,
                    ApprovedCount = stats?.Approved ?? 0,
                    PendingCount = stats?.Pending ?? 0,
                    Status = c.Status == UserStatusEnum.Active ? "Active" : "Disabled",
                    JoinedAt = c.CreatedAt ?? DateTime.UtcNow
                });
            }

            return result;
        }

        private static AdminReviewItemDto MapToReviewItemDto(Node node)
        {
            var firstQuestion = node.NodeQuestions.OrderBy(nq => nq.Order).FirstOrDefault()?.Question;
            var curator = firstQuestion?.Curator;

            // Map Node.Status sang trạng thái chuẩn của Gatekeeper
            var uiStatus = node.Status switch
            {
                "Published" => "APPROVED",
                "Rejected" => "REJECTED",
                "Draft" => "DRAFT",
                _ => "PENDING"
            };

            var questionsList = node.NodeQuestions
                .OrderBy(nq => nq.Order)
                .Where(nq => nq.Question != null)
                .Select(nq => MapToQuestionReviewDto(nq.Question!, nq.Order))
                .ToList();

            return new AdminReviewItemDto
            {
                Id = node.Id,
                LessonTitle = node.Title,
                Category = node.Unit?.Domain?.Name ?? "Kỹ năng tổng quát",
                UnitTitle = node.Unit?.Title,
                CuratorId = curator?.Id,
                CuratorName = curator?.Name ?? firstQuestion?.ReviewedBy ?? "Chuyên gia Mimo",
                CuratorRole = curator?.Role?.Description ?? "Chuyên gia Thẩm định",
                CuratorAvatar = curator?.Avatar,
                SubmittedAt = node.CreatedAt,
                QuestionsCount = questionsList.Count,
                EstimatedMinutes = Math.Max(2, (int)Math.Ceiling(questionsList.Count * 0.5)),
                Status = uiStatus,
                XpReward = node.XpReward,
                ScenarioCover = firstQuestion?.ImageUrl,
                Summary = $"Bài học gồm {questionsList.Count} câu hỏi tình huống thực chiến thuộc chuyên đề '{node.Unit?.Title}'.",
                Questions = questionsList
            };
        }

        private static AdminQuestionReviewDto MapToQuestionReviewDto(Question q, int order)
        {
            var options = new List<string>();
            var correctIndex = -1;
            object? parsedPayload = null;
            object? parsedCorrect = null;

            try
            {
                if (!string.IsNullOrEmpty(q.Payload))
                {
                    using var doc = JsonDocument.Parse(q.Payload);
                    parsedPayload = JsonSerializer.Deserialize<object>(q.Payload);

                    if (doc.RootElement.TryGetProperty("options", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var opt in optionsProp.EnumerateArray())
                        {
                            if (opt.ValueKind == JsonValueKind.String)
                            {
                                options.Add(opt.GetString() ?? string.Empty);
                            }
                            else if (opt.ValueKind == JsonValueKind.Object && opt.TryGetProperty("text", out var textProp))
                            {
                                options.Add(textProp.GetString() ?? string.Empty);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback nếu JSON payload không theo chuẩn mảng
            }

            try
            {
                if (!string.IsNullOrEmpty(q.CorrectData))
                {
                    using var doc = JsonDocument.Parse(q.CorrectData);
                    parsedCorrect = JsonSerializer.Deserialize<object>(q.CorrectData);

                    if (doc.RootElement.TryGetProperty("correctIndex", out var idxProp) && idxProp.TryGetInt32(out var idx))
                    {
                        correctIndex = idx;
                    }
                    else if (doc.RootElement.TryGetProperty("acceptable_indices", out var arrProp) && 
                             arrProp.ValueKind == JsonValueKind.Array && 
                             arrProp.GetArrayLength() > 0)
                    {
                        correctIndex = arrProp[0].GetInt32();
                    }
                }
            }
            catch
            {
                // Fallback
            }

            return new AdminQuestionReviewDto
            {
                Id = q.Id,
                Prompt = q.Title,
                Type = q.Type,
                ImageUrl = q.ImageUrl,
                Payload = parsedPayload,
                CorrectData = parsedCorrect,
                Options = options,
                CorrectIndex = correctIndex,
                Explanation = q.Explanation,
                Difficulty = q.Difficulty,
                Source = q.Source,
                ReviewedBy = q.ReviewedBy,
                Order = order
            };
        }
    }
}
