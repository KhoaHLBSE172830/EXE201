using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Curator;
using Mimo.Application.Middlewares;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin/content-approval")]
    [Produces("application/json")]
    [Protected]
    [RequireRoles(UserRoleEnum.Admin)]
    public class AdminContentApprovalController : ControllerBase
    {
        private readonly IAdminContentService _adminService;

        public AdminContentApprovalController(IAdminContentService adminService)
        {
            _adminService = adminService;
        }

        private Guid GetCurrentUserId()
        {
            if (HttpContext.Items.TryGetValue(JwtConst.PAYLOAD_KEY, out var payloadObj) && payloadObj is Payload payload)
            {
                return payload.UserId;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedException("Không thể xác định danh tính người dùng.");
        }

        [HttpGet]
        [ProducesResponseType(typeof(CuratorLessonNodeDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingLessons(CancellationToken cancellationToken)
        {
            var result = await _adminService.GetPendingLessonsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPost("{nodeId}/publish")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PublishLesson([FromRoute] Guid nodeId, CancellationToken cancellationToken)
        {
            var adminId = GetCurrentUserId();
            await _adminService.PublishLessonAsync(nodeId, adminId, cancellationToken);
            return SuccessResp.Ok("Đã phê duyệt và xuất bản bài học thành công lên bản đồ kỹ năng.");
        }

        [HttpPost("{nodeId}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectLesson([FromRoute] Guid nodeId, [FromBody] AdminRejectLessonRequestDto request, CancellationToken cancellationToken)
        {
            var adminId = GetCurrentUserId();
            await _adminService.RejectLessonAsync(nodeId, adminId, request.Reason, cancellationToken);
            return SuccessResp.Ok("Đã gửi yêu cầu chỉnh sửa kèm biên bản thẩm định cho tác giả.");
        }

        [HttpPost("{nodeId}/emergency-rollback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EmergencyRollback([FromRoute] Guid nodeId, CancellationToken cancellationToken)
        {
            var adminId = GetCurrentUserId();
            await _adminService.RollbackLessonAsync(nodeId, adminId, cancellationToken);
            return SuccessResp.Ok("Đã thu hồi khẩn cấp bài học về trạng thái Draft thành công.");
        }
    }
}
