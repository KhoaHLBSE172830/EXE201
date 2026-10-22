using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Curriculum;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/units")]
    [Produces("application/json")]
    public class UnitsController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public UnitsController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
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
                    // Bỏ qua nếu token không hợp lệ đối với endpoint tùy chọn xác thực
                }
            }

            return null;
        }

        /// <summary>
        /// Task BE2-1: Lấy chi tiết chặng kỹ năng kèm danh sách bài học đã xuất bản
        /// Tự động gắn trạng thái tiến trình (locked | current | done) nếu người dùng đã đăng nhập.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UnitDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUnitById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var userId = TryGetUserId();
            var result = await _curriculumService.GetUnitByIdAsync(id, userId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
