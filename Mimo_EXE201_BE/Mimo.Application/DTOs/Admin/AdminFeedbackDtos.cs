namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO tiếp nhận báo lỗi câu hỏi và góp ý chuyên môn (Feedback Ticket)
    /// </summary>
    public class AdminFeedbackItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = "BUG_UI"; // BUG_UI | MEDICAL_FEEDBACK | TYPO | OTHER
        public string LessonTitle { get; set; } = string.Empty;
        public string QuestionPrompt { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public string UserName => ReportedBy;
        public string UserEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public string Status { get; set; } = "OPEN"; // OPEN | RESOLVED
    }

    /// <summary>
    /// DTO xử lý đóng ticket phản hồi kèm tặng điểm XP tri ân
    /// </summary>
    public class ResolveFeedbackDto
    {
        public int XpGift { get; set; } = 50;
        public string? ThankYouNote { get; set; }
    }
}
