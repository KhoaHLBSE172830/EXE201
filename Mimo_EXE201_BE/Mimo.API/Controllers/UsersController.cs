using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.User;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
        /// Lấy thông tin cá nhân chi tiết, level dẫn xuất từ XP, số tim (Lazy Heart Regeneration), streak
        /// </summary>
        [HttpGet("me")]
        [Protected]
        [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            var result = await _userService.GetMeAsync(userId);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Cập nhật mục tiêu học tập (5/10/15 phút) và kết quả khảo sát nhận diện rủi ro lúc onboarding
        /// </summary>
        [HttpPatch("me/onboarding")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateOnboarding([FromBody] UpdateOnboardingDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UpdateOnboardingAsync(userId, request);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ người dùng (Họ tên, ảnh đại diện, số điện thoại, múi giờ)
        /// </summary>
        [HttpPut("me/profile")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var userId = GetCurrentUserId();
            var result = await _userService.UpdateProfileAsync(userId, request);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Thay đổi mật khẩu tài khoản và tự động thu hồi các phiên đăng nhập khác
        /// </summary>
        [HttpPut("me/change-password")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var userId = GetCurrentUserId();
            await _userService.ChangePasswordAsync(userId, request);
            return SuccessResp.Ok(new { message = "Đổi mật khẩu thành công. Các phiên làm việc khác đã được thu hồi bảo mật." });
        }
    }
}
