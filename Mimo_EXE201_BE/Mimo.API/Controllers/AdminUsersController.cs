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
    /// Phân hệ Admin User Management & CSKH Operations 360
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/users")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
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
        /// Danh sách người dùng toàn hệ thống (Hỗ trợ tìm kiếm, lọc theo gói, vai trò, mất chuỗi)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdminUserItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var result = await _adminUserService.GetUsersAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// CSKH: Cứu chuỗi ngày Streak cho học viên khi gặp sự cố mạng hoặc cúp điện
        /// </summary>
        [HttpPost("{userId:guid}/rescue-streak")]
        [ProducesResponseType(typeof(List<AdminUserItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RescueStreak(Guid userId, [FromBody] RescueStreakDto? request, CancellationToken cancellationToken)
        {
            var result = await _adminUserService.RescueStreakAsync(userId, request?.TargetStreak, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// CSKH: Bơm đầy 5 Trái Tim sinh tồn khẩn cấp cho học viên
        /// </summary>
        [HttpPost("{userId:guid}/refill-hearts")]
        [ProducesResponseType(typeof(List<AdminUserItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefillHearts(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _adminUserService.RefillHeartsAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// CSKH: Tặng điểm XP tri ân/bù lỗi (Ghi nhận trực tiếp vào sổ cái XpEvent bất biến)
        /// </summary>
        [HttpPost("{userId:guid}/reward-xp")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminUserItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RewardXp(Guid userId, [FromBody] RewardUserXpDto request, CancellationToken cancellationToken)
        {
            var adminId = GetCurrentAdminId();
            var result = await _adminUserService.RewardXpAsync(userId, request.Amount, adminId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Khóa (Ban) hoặc Mở khóa (Unban) tài khoản người dùng
        /// </summary>
        [HttpPost("{userId:guid}/toggle-ban")]
        [ProducesResponseType(typeof(List<AdminUserItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleBan(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _adminUserService.ToggleBanAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
