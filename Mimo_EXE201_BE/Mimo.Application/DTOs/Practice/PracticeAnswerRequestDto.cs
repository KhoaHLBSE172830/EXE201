using System;
using System.Text.Json;

namespace Mimo.Application.DTOs.Practice
{
    public class PracticeAnswerRequestDto
    {
        public Guid QuestionId { get; set; }

        public JsonElement SubmittedAnswer { get; set; }
    }
}
