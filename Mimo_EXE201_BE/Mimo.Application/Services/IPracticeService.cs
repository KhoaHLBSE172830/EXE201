using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Practice;

namespace Mimo.Application.Services
{
    public interface IPracticeService
    {
        Task<List<WrongQuestionDto>> GetWrongQuestionsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<PracticeAnswerResponseDto> SubmitPracticeAnswerAsync(Guid userId, PracticeAnswerRequestDto request, CancellationToken cancellationToken = default);
    }
}
