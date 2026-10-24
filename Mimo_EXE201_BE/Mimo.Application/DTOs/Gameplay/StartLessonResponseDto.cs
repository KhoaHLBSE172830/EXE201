using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Gameplay
{
    public class StartLessonResponseDto
    {
        public Guid SessionId { get; set; }

        public Guid NodeId { get; set; }

        public string NodeTitle { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public int HeartsRemaining { get; set; }

        public bool IsPro { get; set; }

        public List<QuestionDisplayDto> Questions { get; set; } = new List<QuestionDisplayDto>();
    }
}
