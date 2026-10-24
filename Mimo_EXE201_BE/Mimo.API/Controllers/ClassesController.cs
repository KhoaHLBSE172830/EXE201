using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Business;
using Mimo.Application.Middlewares;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/classes")]
    [Produces("application/json")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassMembershipService _membershipService;
        private readonly IBusinessService _businessService;

        public ClassesController(
            IClassMembershipService membershipService,
            IBusinessService businessService)
        {
            _membershipService = membershipService;
            _businessService = businessService;
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
        /// Học sinh tham gia lớp học bằng mã code (ClassCode)
        /// </summary>
        [HttpPost("join-by-code")]
        [Protected]
        [ProducesResponseType(typeof(StudentClassroomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinClassByCode([FromBody] JoinClassRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _membershipService.JoinClassByCodeAsync(userId, request.ClassCode, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Học sinh rời khỏi lớp học (tự động hoàn trả ghế bản quyền nếu không còn học lớp nào của trường)
        /// </summary>
        [HttpPost("{classId}/leave")]
        [Protected]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LeaveClass([FromRoute] Guid classId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _membershipService.LeaveClassAsync(userId, classId, cancellationToken);
            return SuccessResp.Ok("Đã rời khỏi lớp học thành công.");
        }

        /// <summary>
        /// Phân tích điểm mù sư phạm / Heatmap câu hỏi sai của lớp (Dành cho Giáo viên & BGH)
        /// </summary>
        [HttpGet("{classId}/heatmap")]
        [Protected]
        [ProducesResponseType(typeof(ClassHeatmapDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassHeatmap([FromRoute] Guid classId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _businessService.GetClassHeatmapAsync(userId, classId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Sổ điểm và tiến độ hoàn thành bài tập của học sinh trong lớp
        /// </summary>
        [HttpGet("{classId}/progress")]
        [Protected]
        [ProducesResponseType(typeof(ClassProgressDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClassProgress([FromRoute] Guid classId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _businessService.GetClassProgressAsync(userId, classId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
