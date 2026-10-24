using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/achievements")]
    [Produces("application/json")]
    public class AchievementsController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementsController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
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

        /// <summary>
        /// Lấy danh sách toàn bộ huy hiệu, tiến độ mở khóa % và ngày đạt của học sinh
        /// </summary>
        [HttpGet]
        [Protected]
        [ProducesResponseType(typeof(BadgesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAchievements()
        {
            var userId = GetCurrentUserId();
            var result = await _achievementService.GetAchievementsAsync(userId);
            return SuccessResp.Ok(result);
        }
    }
}
