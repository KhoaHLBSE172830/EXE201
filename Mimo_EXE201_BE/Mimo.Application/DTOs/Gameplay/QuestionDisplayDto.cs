using System;
using System.Text.Json;

namespace Mimo.Application.DTOs.Gameplay
{
    /// <summary>
    /// DTO hiển thị câu hỏi cho người học — TUYỆT ĐỐI ZERO LEAKAGE (không chứa CorrectData hay IsCorrect)
    /// </summary>
    public class QuestionDisplayDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = "choice";

        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public object? Payload { get; set; }

        public string Difficulty { get; set; } = "Medium";

        public string? Source { get; set; }

        public int Order { get; set; }
    }
}
