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
    [Route("api/v1/quests")]
    [Produces("application/json")]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestService _questService;

        public QuestsController(IQuestService questService)
        {
            _questService = questService;
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
        /// Lấy danh sách nhiệm vụ hàng ngày của học sinh theo múi giờ UTC+7 và thời gian reset lúc 00:00
        /// </summary>
        [HttpGet("daily")]
        [Protected]
        [ProducesResponseType(typeof(DailyQuestsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDailyQuests()
        {
            var userId = GetCurrentUserId();
            var result = await _questService.GetDailyQuestsAsync(userId);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Nhận phần thưởng nhiệm vụ khi đã đạt đủ tiến độ (cộng XP vào sổ cái hoặc tăng bùa/tim)
        /// </summary>
        [HttpPost("{id}/claim")]
        [Protected]
        [ProducesResponseType(typeof(ClaimQuestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClaimQuestReward([FromRoute] Guid id)
        {
            var userId = GetCurrentUserId();
            var result = await _questService.ClaimQuestRewardAsync(userId, id);
            return SuccessResp.Ok(result);
        }
    }
}
