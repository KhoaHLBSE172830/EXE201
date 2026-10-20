using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Gameplay;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/lessons")]
    [Produces("application/json")]
    public class LessonsController : ControllerBase
    {
        private readonly IGameplayService _gameplayService;

        public LessonsController(IGameplayService gameplayService)
        {
            _gameplayService = gameplayService;
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
        /// Task B1: Khởi tạo phiên học mới cho một bài học (Node)
        /// Kiểm tra số Tim (>0 hoặc Pro), tạo LessonSession TTL 30 phút, trả về danh sách câu hỏi không lộ đáp án.
        /// </summary>
        [HttpPost("{nodeId}/start")]
        [Protected]
        [ProducesResponseType(typeof(StartLessonResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartLesson([FromRoute] Guid nodeId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _gameplayService.StartLessonAsync(userId, nodeId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
