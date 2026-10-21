using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Leaderboard;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/leaderboard")]
    [Produces("application/json")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        private Guid? TryGetUserId()
        {
            if (HttpContext.Items.TryGetValue(JwtConst.PAYLOAD_KEY, out var payloadObj) && payloadObj is Payload payload)
            {
                return payload.UserId;
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                try
                {
                    var jwtService = HttpContext.RequestServices.GetService<IJwtService>();
                    var p = jwtService?.ValidateToken(token);
                    if (p != null)
                    {
                        return p.UserId;
                    }
                }
                catch
                {
                    // Cho phép xem ẩn danh bảng xếp hạng nếu không có token hoặc token không hợp lệ
                }
            }

            return null;
        }

        /// <summary>
        /// Task BE5-1: Lấy Bảng xếp hạng tuần hiện tại (Top 30 + vị trí của người dùng hiện tại)
        /// Điểm số được tính toán tổng hợp qua SQL từ bảng sổ cái xp_events bắt đầu từ 00:00:00 Thứ Hai theo múi giờ Việt Nam.
        /// </summary>
        [HttpGet("current")]
        [ProducesResponseType(typeof(LeaderboardResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentLeaderboard(CancellationToken cancellationToken)
        {
            var userId = TryGetUserId();
            var result = await _leaderboardService.GetCurrentLeaderboardAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Task BE5-2: Sinh dữ liệu 35-40 người dùng giả lập có điểm XP ngẫu nhiên trong tuần phục vụ demo hội đồng
        /// [Admin only]
        /// </summary>
        [HttpPost("seed-mock")]
        [RequireRoles(UserRoleEnum.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SeedMockLeaderboard([FromQuery] int count = 35, CancellationToken cancellationToken = default)
        {
            var seeded = await _leaderboardService.SeedMockLeaderboardUsersAsync(count, cancellationToken);
            return SuccessResp.Ok(new { message = $"Đã khởi tạo thành công {seeded} người dùng giả lập đua top tuần." });
        }
    }
}
