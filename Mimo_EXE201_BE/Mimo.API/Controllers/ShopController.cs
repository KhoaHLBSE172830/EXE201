using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/shop")]
    [Produces("application/json")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
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
        /// Lấy danh mục vật phẩm trong cửa hàng và số dư XP, tim, bùa hiện có của người học
        /// </summary>
        [HttpGet("items")]
        [Protected]
        [ProducesResponseType(typeof(ShopItemsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetShopItems()
        {
            var userId = GetCurrentUserId();
            var result = await _shopService.GetShopItemsAsync(userId);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Mua hoặc quy đổi vật phẩm (trừ điểm XP an toàn, ghi nhận sổ cái XpEvents)
        /// </summary>
        [HttpPost("purchase")]
        [Protected]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(PurchaseItemResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PurchaseItem([FromBody] PurchaseItemRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.ItemId))
            {
                throw new BadRequestException("Mã vật phẩm (itemId) không được để trống.");
            }

            var userId = GetCurrentUserId();
            var result = await _shopService.PurchaseItemAsync(userId, request.ItemId);
            return SuccessResp.Ok(result);
        }
    }
}
