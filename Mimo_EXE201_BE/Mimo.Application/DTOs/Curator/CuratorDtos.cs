using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Curator
{
    public class CuratorProfileDto
    {
        public Guid CuratorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialty { get; set; }
        public int DraftCount { get; set; }
        public int PendingCount { get; set; }
        public int PublishedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalQuestions { get; set; }
    }

    public class CuratorQuestionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "choice";
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public object? Payload { get; set; }
        public object? CorrectData { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string? Source { get; set; }
        public string? ReviewedBy { get; set; }
        public string Status { get; set; } = "Draft";
        public Guid? CuratorId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCuratorQuestionRequestDto
    {
        public string Type { get; set; } = "choice";
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Payload { get; set; } = "{}";
        public string CorrectData { get; set; } = "{}";
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string? Source { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class UpdateCuratorQuestionRequestDto
    {
        public string Type { get; set; } = "choice";
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Payload { get; set; } = "{}";
        public string CorrectData { get; set; } = "{}";
        public string Explanation { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "Medium";
        public string? Source { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class CuratorLessonNodeDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string UnitTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "lesson";
        public int Order { get; set; }
        public string Status { get; set; } = "Draft";
        public int XpReward { get; set; } = 10;
        public int QuestionsCount { get; set; }
        public Guid? CuratorId { get; set; }
        public List<CuratorQuestionDto> Questions { get; set; } = new();
    }

    public class CreateCuratorLessonRequestDto
    {
        public Guid UnitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; } = 1;
        public int XpReward { get; set; } = 10;
    }

    public class UpdateLessonQuestionsRequestDto
    {
        public List<Guid> QuestionIds { get; set; } = new();
    }

    public class CuratorPreviewAnswerRequestDto
    {
        public string QuestionType { get; set; } = "choice";
        public string SubmittedAnswerJson { get; set; } = "{}";
        public string CorrectDataJson { get; set; } = "{}";
    }

    public class CuratorPreviewAnswerResponseDto
    {
        public bool IsCorrect { get; set; }
    }

    public class CuratorFeedbackDto
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public string Kind { get; set; } = "Reject";
        public string Note { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class ResolveFeedbackRequestDto
    {
        public string? ResolutionNote { get; set; }
    }

    public class CuratorAnalyticsItemDto
    {
        public Guid QuestionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int WrongAttempts { get; set; }
        public double WrongRate { get; set; }
    }

    public class AdminRejectLessonRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}
