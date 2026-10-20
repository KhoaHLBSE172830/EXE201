using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Practice;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/practice")]
    [Produces("application/json")]
    public class PracticeController : ControllerBase
    {
        private readonly IPracticeService _practiceService;

        public PracticeController(IPracticeService practiceService)
        {
            _practiceService = practiceService;
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
        /// Task B5: Lấy danh sách các câu hỏi đã từng làm sai cần ôn tập
        /// Ẩn đáp án đúng (Zero Leakage).
        /// </summary>
        [HttpGet("wrong-questions")]
        [Protected]
        [ProducesResponseType(typeof(List<WrongQuestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWrongQuestions(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _practiceService.GetWrongQuestionsAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Task B5: Nộp câu trả lời trong phòng ôn tập
        /// Nếu làm đúng: hồi phục +1 Tim (tối đa 5), thưởng +5 XP, đánh dấu câu hỏi đã giải quyết.
        /// Nếu làm sai: không trừ tim.
        /// </summary>
        [HttpPost("answer")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PracticeAnswerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitPracticeAnswer(
            [FromBody] PracticeAnswerRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _practiceService.SubmitPracticeAnswerAsync(userId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
