using System;

namespace Mimo.Application.DTOs.Practice
{
    public class WrongQuestionDto
    {
        public Guid Id { get; set; }

        public Guid QuestionId { get; set; }

        public string Type { get; set; } = "choice";

        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public object? Payload { get; set; }

        public int TimesWrong { get; set; }

        public DateTime LastAttemptAt { get; set; }
    }
}
