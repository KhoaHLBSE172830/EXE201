using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Curriculum;

namespace Mimo.Application.Services
{
    public interface ICurriculumService
    {
        /// <summary>
        /// Task BE2-1: Lấy danh sách 5 chủ đề lớn (Domain)
        /// </summary>
        Task<List<DomainOverviewDto>> GetDomainsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy thông tin chi tiết một chủ đề kèm các chặng kỹ năng
        /// </summary>
        Task<DomainDto> GetDomainByIdAsync(Guid domainId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Task BE2-1: Lấy thông tin chặng kỹ năng kèm danh sách bài học trực thuộc
        /// </summary>
        Task<UnitDto> GetUnitByIdAsync(Guid unitId, Guid? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Task BE2-2: Lấy bản đồ học tập (Learning Path) kèm trạng thái cá nhân (locked | current | done)
        /// </summary>
        Task<LearningPathDto> GetLearningPathAsync(Guid? domainId, Guid? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết thông tin một nút bài học (không lộ ngân hàng câu hỏi)
        /// </summary>
        Task<NodeDto> GetNodeDetailAsync(Guid nodeId, Guid? userId = null, CancellationToken cancellationToken = default);
    }
}
