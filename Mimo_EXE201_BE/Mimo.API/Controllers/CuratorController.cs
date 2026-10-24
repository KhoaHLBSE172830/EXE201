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
    [Route("api/v1/curator")]
    [Produces("application/json")]
    [Protected]
    [RequireRoles(UserRoleEnum.Curator, UserRoleEnum.Admin)]
    public class CuratorController : ControllerBase
    {
        private readonly ICuratorContentService _curatorService;

        public CuratorController(ICuratorContentService curatorService)
        {
            _curatorService = curatorService;
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

        [HttpGet("profile")]
        [ProducesResponseType(typeof(CuratorProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetProfileAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpGet("questions")]
        [ProducesResponseType(typeof(CuratorQuestionDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuestions([FromQuery] string? status, [FromQuery] string? type, [FromQuery] string? difficulty, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetQuestionsAsync(userId, status, type, difficulty, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpGet("questions/{id}")]
        [ProducesResponseType(typeof(CuratorQuestionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuestionById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetQuestionByIdAsync(userId, id, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPost("questions")]
        [ProducesResponseType(typeof(CuratorQuestionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateCuratorQuestionRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.CreateQuestionAsync(userId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPut("questions/{id}")]
        [ProducesResponseType(typeof(CuratorQuestionDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateQuestion([FromRoute] Guid id, [FromBody] UpdateCuratorQuestionRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.UpdateQuestionAsync(userId, id, request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpDelete("questions/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteQuestion([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _curatorService.DeleteQuestionAsync(userId, id, cancellationToken);
            return SuccessResp.Ok("Đã xóa câu hỏi thành công.");
        }

        [HttpPost("questions/{id}/submit-approval")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitQuestion([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _curatorService.SubmitQuestionForApprovalAsync(userId, id, cancellationToken);
            return SuccessResp.Ok("Đã gửi câu hỏi lên Hội Đồng Chuyên Môn để thẩm định.");
        }

        [HttpGet("lessons")]
        [ProducesResponseType(typeof(CuratorLessonNodeDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLessons(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetLessonsAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPost("lessons")]
        [ProducesResponseType(typeof(CuratorLessonNodeDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateLesson([FromBody] CreateCuratorLessonRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.CreateLessonAsync(userId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPut("lessons/{nodeId}/questions")]
        [ProducesResponseType(typeof(CuratorLessonNodeDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLessonQuestions([FromRoute] Guid nodeId, [FromBody] UpdateLessonQuestionsRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.UpdateLessonQuestionsAsync(userId, nodeId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPost("lessons/{nodeId}/submit-approval")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SubmitLesson([FromRoute] Guid nodeId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _curatorService.SubmitLessonForApprovalAsync(userId, nodeId, cancellationToken);
            return SuccessResp.Ok("Đã gửi trọn gói bài học lên Hội Đồng Thẩm Định thành công.");
        }

        [HttpPost("preview/answer")]
        [ProducesResponseType(typeof(CuratorPreviewAnswerResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> PreviewAnswer([FromBody] CuratorPreviewAnswerRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _curatorService.PreviewAnswerAsync(request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpGet("analytics")]
        [ProducesResponseType(typeof(CuratorAnalyticsItemDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAnalytics(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetAnalyticsAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpGet("feedback")]
        [ProducesResponseType(typeof(CuratorFeedbackDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeedbacks(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _curatorService.GetFeedbacksAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        [HttpPost("feedback/{id}/resolve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResolveFeedback([FromRoute] Guid id, [FromBody] ResolveFeedbackRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _curatorService.ResolveFeedbackAsync(userId, id, request, cancellationToken);
            return SuccessResp.Ok("Đã đánh dấu xử lý phản hồi thành công.");
        }
    }
}
