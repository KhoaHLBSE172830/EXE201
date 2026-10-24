using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminFeedbackService
    {
        Task<List<AdminFeedbackItemDto>> GetFeedbacksAsync(CancellationToken cancellationToken = default);
        Task<List<AdminFeedbackItemDto>> ResolveFeedbackAsync(string feedbackId, int xpGift, Guid adminUserId, CancellationToken cancellationToken = default);
    }
}
