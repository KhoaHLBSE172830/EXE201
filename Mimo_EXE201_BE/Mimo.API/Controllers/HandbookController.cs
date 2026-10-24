using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs.Gamification;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/handbook")]
    [Produces("application/json")]
    public class HandbookController : ControllerBase
    {
        private readonly IHandbookService _handbookService;

        public HandbookController(IHandbookService handbookService)
        {
            _handbookService = handbookService;
        }

        /// <summary>
        /// Lấy danh mục cẩm nang an toàn sống, các trụ cột kỹ năng và 4 hotline cứu hộ quốc gia (111, 113, 114, 115)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(HandbookResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHandbook([FromQuery] string? pillar, [FromQuery] string? search)
        {
            var result = await _handbookService.GetHandbookAsync(pillar, search);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết bài viết cẩm nang chuẩn y khoa / căn cứ luật định
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HandbookArticleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById([FromRoute] string id)
        {
            var result = await _handbookService.GetArticleByIdAsync(id);
            return SuccessResp.Ok(result);
        }
    }
}
