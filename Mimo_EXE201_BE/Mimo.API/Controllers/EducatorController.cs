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
    [Route("api/v1/educator")]
    [Produces("application/json")]
    [Protected]
    public class EducatorController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public EducatorController(IBusinessService businessService)
        {
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
        /// Danh sách các lớp học do Giáo viên phụ trách
        /// </summary>
        [HttpGet("classes")]
        [ProducesResponseType(typeof(ClassDto[]), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyClasses(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _businessService.GetEducatorClassesAsync(userId, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Kích hoạt mã bản quyền Giáo viên (nhận quyền quản trị lớp học của trường)
        /// </summary>
        [HttpPost("claim-license")]
        [ProducesResponseType(typeof(ClaimLicenseResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClaimLicense([FromBody] ClaimLicenseRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _businessService.ClaimTeacherKeyAsync(userId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
