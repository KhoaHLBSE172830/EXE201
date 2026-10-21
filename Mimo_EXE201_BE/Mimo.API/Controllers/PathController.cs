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
    [Route("api/v1/path")]
    [Produces("application/json")]
    public class PathController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public PathController(ICurriculumService curriculumService)
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
                    // Cho phép truy cập ẩn danh (preview lộ trình) nếu không có hoặc token hết hạn
                }
            }

            return null;
        }

        /// <summary>
        /// Task BE2-2: Lấy bản đồ lộ trình học tập (Learning Path / Skill Tree)
        /// Trả về cây bài học phân cấp theo Domain -> Unit -> Node kèm trạng thái cá nhân hóa (locked | current | done).
        /// Nếu không truyền domainId, mặc định lấy chủ đề đầu tiên đang kích hoạt.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(LearningPathDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLearningPath([FromQuery] Guid? domainId, CancellationToken cancellationToken)
        {
            var userId = TryGetUserId();
            var result = await _curriculumService.GetLearningPathAsync(domainId, userId, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
