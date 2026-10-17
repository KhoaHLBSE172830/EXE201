using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Auth;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng ký tài khoản mới (Hỗ trợ 4 Roles: Admin, Student, Curator, Organization)
        /// </summary>
        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Đăng nhập bằng Email và Password
        /// </summary>
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Lấy thông tin tài khoản đang đăng nhập (Yêu cầu Bearer Token)
        /// </summary>
        [HttpGet("me")]
        [Protected]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Lấy userId từ HttpContext Items đã được middleware/filter giải mã
            if (HttpContext.Items.TryGetValue(JwtConst.PAYLOAD_KEY, out var payloadObj) && payloadObj is Payload payload)
            {
                var user = await _authService.GetCurrentUserAsync(payload.UserId);
                return SuccessResp.Ok(user);
            }

            // Fallback: Lấy từ ClaimsPrincipal của .NET
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("userId")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                var user = await _authService.GetCurrentUserAsync(userId);
                return SuccessResp.Ok(user);
            }

            return ErrorResp.Unauthorized("Không thể xác định danh tính người dùng.");
        }

        /// <summary>
        /// Cấp lại Access Token mới bằng Refresh Token
        /// </summary>
        [HttpPost("refresh-token")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Endpoint kiểm tra phân quyền riêng cho Role Admin
        /// </summary>
        [HttpGet("admin-only")]
        [RequireRoles(UserRoleEnum.Admin)]
        public IActionResult AdminOnly()
        {
            return SuccessResp.Ok(new { message = "Xin chào Admin! Bạn có toàn quyền quản trị hệ thống MIMO." });
        }

        /// <summary>
        /// Endpoint kiểm tra phân quyền cho Role Curator hoặc Admin
        /// </summary>
        [HttpGet("curator-only")]
        [RequireRoles(UserRoleEnum.Curator, UserRoleEnum.Admin)]
        public IActionResult CuratorOnly()
        {
            return SuccessResp.Ok(new { message = "Xin chào Giảng viên / Curator! Bạn có quyền quản lý bài giảng & kịch bản." });
        }

        /// <summary>
        /// Endpoint kiểm tra phân quyền cho Role Organization hoặc Admin
        /// </summary>
        [HttpGet("organization-only")]
        [RequireRoles(UserRoleEnum.Organization, UserRoleEnum.Admin)]
        public IActionResult OrganizationOnly()
        {
            return SuccessResp.Ok(new { message = "Xin chào Tổ chức B2B! Bạn có quyền quản trị trường học và lớp học." });
        }
    }
}
