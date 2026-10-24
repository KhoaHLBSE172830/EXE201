using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Admin;

namespace Mimo.Application.Services
{
    public interface IAdminContentService
    {
        Task<List<AdminReviewItemDto>> GetReviewsAsync(string? status = null, CancellationToken cancellationToken = default);
        Task<AdminReviewItemDto> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<AdminReviewItemDto>> ApproveReviewAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<AdminReviewItemDto>> RejectReviewAsync(Guid id, string reason, CancellationToken cancellationToken = default);
        Task<List<AdminReviewItemDto>> RollbackReviewAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<AdminCuratorDto>> GetCuratorsAsync(CancellationToken cancellationToken = default);
    }
}
