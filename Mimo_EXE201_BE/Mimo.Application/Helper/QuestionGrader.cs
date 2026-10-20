using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Mimo.Application.Helper
{
    public interface IQuestionGrader
    {
        bool Grade(string questionType, string submittedAnswerJson, string correctDataJson);
    }

    public class QuestionGrader : IQuestionGrader
    {
        public bool Grade(string questionType, string submittedAnswerJson, string correctDataJson)
        {
            if (string.IsNullOrWhiteSpace(submittedAnswerJson) || string.IsNullOrWhiteSpace(correctDataJson))
            {
                return false;
            }

            try
            {
                using var submittedDoc = JsonDocument.Parse(submittedAnswerJson);
                using var correctDoc = JsonDocument.Parse(correctDataJson);

                return questionType.ToLowerInvariant() switch
                {
                    "choice" => GradeChoice(submittedDoc.RootElement, correctDoc.RootElement),
                    "sequence" or "sequencing" => GradeSequencing(submittedDoc.RootElement, correctDoc.RootElement),
                    "red_flag" or "redflag" => GradeRedFlag(submittedDoc.RootElement, correctDoc.RootElement),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private static bool GradeChoice(JsonElement submitted, JsonElement correct)
        {
            var submittedVal = ExtractStringValue(submitted, "selectedOptionId", "optionId", "choiceId", "answer");
            var correctVal = ExtractStringValue(correct, "correctOptionId", "optionId", "correctId", "answer");

            if (string.IsNullOrEmpty(submittedVal) || string.IsNullOrEmpty(correctVal))
            {
                return false;
            }

            return string.Equals(submittedVal.Trim(), correctVal.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool GradeSequencing(JsonElement submitted, JsonElement correct)
        {
            var submittedList = ExtractStringList(submitted, "orderedItemIds", "items", "correctOrder", "order");
            var correctList = ExtractStringList(correct, "correctOrder", "orderedItemIds", "order", "items");

            if (submittedList == null || correctList == null || submittedList.Count == 0)
            {
                return false;
            }

            if (submittedList.Count != correctList.Count)
            {
                return false;
            }

            for (int i = 0; i < submittedList.Count; i++)
            {
                if (!string.Equals(submittedList[i]?.Trim(), correctList[i]?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool GradeRedFlag(JsonElement submitted, JsonElement correct)
        {
            var submittedVal = ExtractStringValue(submitted, "selectedFlagId", "flagId", "key", "answer");
            var correctVal = ExtractStringValue(correct, "selectedFlagId", "flagId", "key", "answer");

            if (string.IsNullOrEmpty(submittedVal) || string.IsNullOrEmpty(correctVal))
            {
                return false;
            }

            return string.Equals(submittedVal.Trim(), correctVal.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string? ExtractStringValue(JsonElement element, params string[] propertyNames)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in propertyNames)
                {
                    if (element.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                    {
                        return val.GetString();
                    }
                }
            }

            return null;
        }

        private static List<string>? ExtractStringList(JsonElement element, params string[] propertyNames)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString()!)
                    .ToList();
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in propertyNames)
                {
                    if (element.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.Array)
                    {
                        return val.EnumerateArray()
                            .Where(x => x.ValueKind == JsonValueKind.String)
                            .Select(x => x.GetString()!)
                            .ToList();
                    }
                }
            }

            return null;
        }
    }
}
