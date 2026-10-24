using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs.Admin;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    /// <summary>
    /// Phân hệ Admin Gamification Behavioral Balancing Engine (Cân bằng cơ chế game hóa)
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/settings")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminGamificationController : ControllerBase
    {
        private readonly IAdminGamificationService _gamificationService;

        public AdminGamificationController(IAdminGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        /// <summary>
        /// Lấy toàn bộ tham số cân bằng game hóa hiện tại (Tim, Streak, League, XP)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(GamificationSettingsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var result = await _gamificationService.GetSettingsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Lưu cấu hình cân bằng tham số game hóa mới (lưu vào Cache Redis & hệ thống)
        /// </summary>
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(GamificationSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveSettings([FromBody] GamificationSettingsDto settings, CancellationToken cancellationToken)
        {
            var result = await _gamificationService.SaveSettingsAsync(settings, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Khôi phục toàn bộ tham số Game hóa về mặc định ban đầu của MIMO
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(GamificationSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetSettings(CancellationToken cancellationToken)
        {
            var result = await _gamificationService.ResetSettingsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
