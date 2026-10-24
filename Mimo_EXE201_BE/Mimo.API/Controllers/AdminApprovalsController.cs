using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    /// <summary>
    /// Phân hệ Admin Gatekeeper: Thẩm định xuất bản học liệu, từ chối, thu hồi khẩn cấp và quản lý Curator
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/approvals")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminApprovalsController : ControllerBase
    {
        private readonly IAdminContentService _adminContentService;

        public AdminApprovalsController(IAdminContentService adminContentService)
        {
            _adminContentService = adminContentService;
        }

        /// <summary>
        /// Lấy hàng chờ thẩm định bài học/node (Gatekeeper Queue) kèm lọc theo trạng thái
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdminReviewItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetReviews([FromQuery] string? status, CancellationToken cancellationToken)
        {
            var result = await _adminContentService.GetReviewsAsync(status, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Xem chi tiết một bài học và danh sách câu hỏi kèm đáp án đúng để Admin thẩm định
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AdminReviewItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _adminContentService.GetReviewByIdAsync(id, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Phê duyệt xuất bản bài học (Chuyển sang trạng thái Published để live toàn quốc)
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(typeof(List<AdminReviewItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveReview(Guid id, CancellationToken cancellationToken)
        {
            var result = await _adminContentService.ApproveReviewAsync(id, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Từ chối xuất bản bài học kèm lý do yêu cầu Curator chỉnh sửa
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminReviewItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectReview(Guid id, [FromBody] RejectReviewDto request, CancellationToken cancellationToken)
        {
            var result = await _adminContentService.RejectReviewAsync(id, request.Reason, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Thu hồi khẩn cấp (Emergency Rollback) bài học live về Draft khi phát hiện sai lệch phác đồ
        /// </summary>
        [HttpPost("{id:guid}/rollback")]
        [ProducesResponseType(typeof(List<AdminReviewItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RollbackReview(Guid id, CancellationToken cancellationToken)
        {
            var result = await _adminContentService.RollbackReviewAsync(id, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Danh sách các chuyên gia biên soạn độc lập (Curators)
        /// </summary>
        [HttpGet("~/api/v1/admin/curators")]
        [ProducesResponseType(typeof(List<AdminCuratorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurators(CancellationToken cancellationToken)
        {
            var result = await _adminContentService.GetCuratorsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
