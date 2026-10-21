using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs.Curriculum;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/domains")]
    [Produces("application/json")]
    public class DomainsController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public DomainsController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
        }

        /// <summary>
        /// Task BE2-1: Lấy danh sách các chủ đề kỹ năng lớn (Domains)
        /// Trả về 5 chủ đề an toàn và kỹ năng sống kèm số lượng chặng (Units) và bài học (Nodes).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DomainOverviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDomains(CancellationToken cancellationToken)
        {
            var result = await _curriculumService.GetDomainsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Task BE2-1: Lấy thông tin chi tiết một chủ đề kèm danh sách các chặng kỹ năng
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DomainDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDomainById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _curriculumService.GetDomainByIdAsync(id, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
