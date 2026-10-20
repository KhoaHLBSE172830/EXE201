using System;

namespace Mimo.Application.DTOs.Practice
{
    public class PracticeAnswerResponseDto
    {
        public Guid QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public int HeartsRemaining { get; set; }

        public int HeartRecovered { get; set; }

        public int XpEarned { get; set; }
    }
}
