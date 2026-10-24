using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Curator;

namespace Mimo.Application.Services
{
    public interface ICuratorContentService
    {
        Task<CuratorProfileDto> GetProfileAsync(Guid curatorId, CancellationToken cancellationToken = default);
        Task<List<CuratorQuestionDto>> GetQuestionsAsync(Guid curatorId, string? status = null, string? type = null, string? difficulty = null, CancellationToken cancellationToken = default);
        Task<CuratorQuestionDto> GetQuestionByIdAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default);
        Task<CuratorQuestionDto> CreateQuestionAsync(Guid curatorId, CreateCuratorQuestionRequestDto request, CancellationToken cancellationToken = default);
        Task<CuratorQuestionDto> UpdateQuestionAsync(Guid curatorId, Guid questionId, UpdateCuratorQuestionRequestDto request, CancellationToken cancellationToken = default);
        Task DeleteQuestionAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default);
        Task SubmitQuestionForApprovalAsync(Guid curatorId, Guid questionId, CancellationToken cancellationToken = default);

        Task<List<CuratorLessonNodeDto>> GetLessonsAsync(Guid curatorId, CancellationToken cancellationToken = default);
        Task<CuratorLessonNodeDto> CreateLessonAsync(Guid curatorId, CreateCuratorLessonRequestDto request, CancellationToken cancellationToken = default);
        Task<CuratorLessonNodeDto> UpdateLessonQuestionsAsync(Guid curatorId, Guid nodeId, UpdateLessonQuestionsRequestDto request, CancellationToken cancellationToken = default);
        Task SubmitLessonForApprovalAsync(Guid curatorId, Guid nodeId, CancellationToken cancellationToken = default);

        Task<CuratorPreviewAnswerResponseDto> PreviewAnswerAsync(CuratorPreviewAnswerRequestDto request, CancellationToken cancellationToken = default);
        Task<List<CuratorAnalyticsItemDto>> GetAnalyticsAsync(Guid curatorId, CancellationToken cancellationToken = default);
        Task<List<CuratorFeedbackDto>> GetFeedbacksAsync(Guid curatorId, CancellationToken cancellationToken = default);
        Task ResolveFeedbackAsync(Guid curatorId, Guid feedbackId, ResolveFeedbackRequestDto request, CancellationToken cancellationToken = default);
    }
}
