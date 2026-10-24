using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Curator;

namespace Mimo.Application.Services
{
    public interface IAdminContentService
    {
        Task<List<CuratorLessonNodeDto>> GetPendingLessonsAsync(CancellationToken cancellationToken = default);
        Task PublishLessonAsync(Guid nodeId, Guid adminId, CancellationToken cancellationToken = default);
        Task RejectLessonAsync(Guid nodeId, Guid adminId, string reason, CancellationToken cancellationToken = default);
        Task RollbackLessonAsync(Guid nodeId, Guid adminId, CancellationToken cancellationToken = default);
    }
}
