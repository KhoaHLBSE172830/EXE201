using System;
using System.Collections.Generic;
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
    /// Phân hệ Admin B2B School & Seat Licenses Management
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/schools")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminSchoolsController : ControllerBase
    {
        private readonly IAdminSchoolService _adminSchoolService;

        public AdminSchoolsController(IAdminSchoolService adminSchoolService)
        {
            _adminSchoolService = adminSchoolService;
        }

        /// <summary>
        /// Danh sách các trường học đối tác B2B và tình trạng sử dụng Seat bản quyền
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdminSchoolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSchools(CancellationToken cancellationToken)
        {
            var result = await _adminSchoolService.GetSchoolsAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Tạo mới đối tác trường học B2B và cấp hạn mức Seat bản quyền
        /// </summary>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminSchoolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSchoolContract([FromBody] CreateSchoolContractDto request, CancellationToken cancellationToken)
        {
            var result = await _adminSchoolService.CreateSchoolContractAsync(request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Cập nhật hợp đồng trường B2B (gia hạn ngày hết hạn, điều chỉnh số Seat)
        /// </summary>
        [HttpPut("{schoolId:guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminSchoolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSchoolContract(Guid schoolId, [FromBody] UpdateSchoolContractDto request, CancellationToken cancellationToken)
        {
            var result = await _adminSchoolService.UpdateSchoolContractAsync(schoolId, request, cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Sinh dải mã lớp học / mã kích hoạt giáo viên mới cho trường
        /// </summary>
        [HttpPost("{schoolId:guid}/generate-classes")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(List<AdminSchoolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GenerateClassCodes(Guid schoolId, [FromBody] GenerateClassCodesDto request, CancellationToken cancellationToken)
        {
            var result = await _adminSchoolService.GenerateClassCodesAsync(schoolId, request.Count, cancellationToken);
            return SuccessResp.Ok(result);
        }
    }
}
