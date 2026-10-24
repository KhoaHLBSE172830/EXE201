using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    /// <summary>
    /// Phân hệ Admin tiếp nhận phản hồi, sự cố và góp ý chuyên môn (Feedback & Incidents)
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/feedbacks")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminFeedbacksController : ControllerBase
    {
        private readonly IAdminFeedbackService _feedbackService;

        public AdminFeedbacksController(IAdminFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        private Guid GetCurrentAdminId()
        {
            if (HttpContext.Items.TryGetValue(JwtConst.PAYLOAD_KEY, out var payloadObj) && payloadObj is Payload payload)
            {
                return payload.UserId;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("userId")?.Value;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        /// <summary>
        /// Danh sách phản hồi và phiếu báo lỗi kịch bản học liệu từ người học
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdminFeedbackItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFeedbacks(CancellationToken cancellationToken)
        {
            var result = await _feedbackService.GetFeedbacksAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Xử lý đóng phiếu phản hồi kèm tùy chọn gửi tặng điểm XP tri ân
        /// </summary>
        [HttpPost("{id}/resolve")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminFeedbackItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResolveFeedback(string id, [FromBody] ResolveFeedbackDto request, CancellationToken cancellationToken)
        {
            var adminId = GetCurrentAdminId();
            var result = await _feedbackService.ResolveFeedbackAsync(id, request.XpGift, adminId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
