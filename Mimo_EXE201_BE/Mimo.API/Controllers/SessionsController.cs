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
    [Route("api/v1/sessions")]
    [Produces("application/json")]
    public class SessionsController : ControllerBase
    {
        private readonly IGameplayService _gameplayService;

        public SessionsController(IGameplayService gameplayService)
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
        /// Task B2: Engine chấm điểm câu hỏi trong phiên học
        /// Hỗ trợ 3 dạng câu hỏi (Choice, Sequencing, Red Flag). Trừ tim nếu sai, cộng XP + combo nếu đúng.
        /// Hỗ trợ header Idempotency-Key chống nộp 2 lần.
        /// </summary>
        [HttpPost("{id}/answer")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(SubmitAnswerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitAnswer(
            [FromRoute] Guid id,
            [FromBody] SubmitAnswerRequestDto request,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _gameplayService.SubmitAnswerAsync(userId, id, request, idempotencyKey, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Task B4: Tổng kết phiên học
        /// Thưởng Perfect Lesson (+5 XP nếu hearts_lost == 0), chốt phiên, mở khóa node tiếp theo và cập nhật Streak.
        /// </summary>
        [HttpPost("{id}/complete")]
        [Protected]
        [ProducesResponseType(typeof(CompleteSessionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteSession(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _gameplayService.CompleteSessionAsync(userId, id, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
