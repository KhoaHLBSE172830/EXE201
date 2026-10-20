using System;

namespace Mimo.Application.DTOs.Gameplay
{
    public class SubmitAnswerResponseDto
    {
        public Guid QuestionId { get; set; }

        public bool IsCorrect { get; set; }

        public string Explanation { get; set; } = string.Empty;

        public int HeartsRemaining { get; set; }

        public int CurrentComboStreak { get; set; }

        public int XpEarned { get; set; }

        public int ComboBonusXp { get; set; }

        public bool IsSessionFailed { get; set; }

        public bool IdempotentReplay { get; set; }
    }
}
