using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO đại diện cho một bài học/node trong hàng chờ thẩm định của Admin (Gatekeeper Queue)
    /// Khớp với frontend AdminApprovalsPage và mockAdmin
    /// </summary>
    public class AdminReviewItemDto
    {
        public Guid Id { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? UnitTitle { get; set; }
        public Guid? CuratorId { get; set; }
        public string CuratorName { get; set; } = "Chuyên gia Mimo";
        public string CuratorRole { get; set; } = "Chuyên gia Thẩm định";
        public string? CuratorAvatar { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int QuestionsCount { get; set; }
        public int EstimatedMinutes { get; set; } = 3;
        /// <summary>
        /// PENDING, APPROVED, REJECTED, DRAFT
        /// </summary>
        public string Status { get; set; } = "PENDING";
        public int XpReward { get; set; } = 35;
        public string? ScenarioCover { get; set; }
        public string? Summary { get; set; }
        public string? RejectionReason { get; set; }
        public List<AdminQuestionReviewDto> Questions { get; set; } = new();
    }

    /// <summary>
    /// DTO chi tiết câu hỏi dành cho Admin duyệt (Admin ĐƯỢC PHÉP xem CorrectData và Explanation để thẩm định chuyên môn)
    /// </summary>
    public class AdminQuestionReviewDto
    {
        public Guid Id { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string Type { get; set; } = "choice";
        public string? ImageUrl { get; set; }
        public object? Payload { get; set; }
        public object? CorrectData { get; set; }
        public List<string> Options { get; set; } = new();
        public int CorrectIndex { get; set; } = -1;
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string? Source { get; set; }
        public string? ReviewedBy { get; set; }
        public int Order { get; set; }
    }

    /// <summary>
    /// DTO gửi lý do từ chối bài học
    /// </summary>
    public class RejectReviewDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO thông tin chuyên gia biên soạn độc lập (Curator)
    /// </summary>
    public class AdminCuratorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Organization { get; set; } = "Độc lập";
        public string? Avatar { get; set; }
        public string AssignedDomain { get; set; } = string.Empty;
        public int TotalSubmitted { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingCount { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime JoinedAt { get; set; }
    }
}
