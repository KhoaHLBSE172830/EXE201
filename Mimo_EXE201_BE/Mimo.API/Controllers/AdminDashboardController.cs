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
    /// Phân hệ Admin Dashboard, Chỉ số sống còn (North Star Metrics) và Cảnh báo an toàn
    /// </summary>
    [ApiController]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Lấy bảng chỉ số sống còn thời gian thực (DAU, MAU, Stickiness, Lives Saved, MRR)
        /// </summary>
        [HttpGet("api/v1/admin/analytics")]
        [HttpGet("api/v1/admin/dashboard")]
        [ProducesResponseType(typeof(AdminAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetAnalyticsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Bản đồ nhiệt vùng mù kiến thức an toàn (Safety Skill Gap Heatmap)
        /// </summary>
        [HttpGet("api/v1/admin/skill-gaps")]
        [ProducesResponseType(typeof(List<AdminSkillGapDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSkillGaps(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetSkillGapsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Phát thông báo cảnh báo kỹ năng khẩn cấp toàn quốc tới 100% người học
        /// </summary>
        [HttpPost("api/v1/admin/broadcast-alert")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BroadcastSkillAlert([FromBody] BroadcastAlertRequestDto request, CancellationToken cancellationToken)
        {
            await _dashboardService.BroadcastAlertAsync(request, cancellationToken);
            return SuccessResp.Ok(new { message = "Đã phát sóng cảnh báo kỹ năng khẩn cấp toàn quốc thành công." });
        }
    }
}
