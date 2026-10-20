using System;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Gameplay;

namespace Mimo.Application.Services
{
    public interface IGameplayService
    {
        Task<StartLessonResponseDto> StartLessonAsync(Guid userId, Guid nodeId, CancellationToken cancellationToken = default);

        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(Guid userId, Guid sessionId, SubmitAnswerRequestDto request, string? idempotencyKey, CancellationToken cancellationToken = default);

        Task<CompleteSessionResponseDto> CompleteSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    }
}
