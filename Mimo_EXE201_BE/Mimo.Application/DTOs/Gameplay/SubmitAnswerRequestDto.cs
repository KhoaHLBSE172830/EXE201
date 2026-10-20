using System;
using System.Text.Json;

namespace Mimo.Application.DTOs.Gameplay
{
    public class SubmitAnswerRequestDto
    {
        public Guid QuestionId { get; set; }

        /// <summary>
        /// Dữ liệu nộp câu trả lời (JSON dynamic):
        /// - Choice: { "selectedOptionId": "B" }
        /// - Sequencing: { "orderedItemIds": ["step1", "step2", ...] }
        /// - Red Flag: { "selectedFlagId": "phishing_link" }
        /// </summary>
        public JsonElement SubmittedAnswer { get; set; }

        public int TimeSpentSeconds { get; set; } = 0;
    }
}
