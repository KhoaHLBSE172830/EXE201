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
    /// Phân hệ Admin Monetization, Pricing & Payment Gateway Operations
    /// </summary>
    [ApiController]
    [Route("api/v1/admin/monetization")]
    [RequireRoles(UserRoleEnum.Admin)]
    [Produces("application/json")]
    public class AdminMonetizationController : ControllerBase
    {
        private readonly IAdminMonetizationService _monetizationService;

        public AdminMonetizationController(IAdminMonetizationService monetizationService)
        {
            _monetizationService = monetizationService;
        }

        /// <summary>
        /// Báo cáo tài chính doanh thu đa nguồn (MRR/ARR), bảng giá gói cước và lịch sử giao dịch
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AdminMonetizationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMonetization(CancellationToken cancellationToken)
        {
            var result = await _monetizationService.GetMonetizationAsync(cancellationToken);
            return SuccessResp.Ok(result);
        }

        /// <summary>
        /// Thực hiện lệnh hoàn tiền (Refund) cho giao dịch thanh toán
        /// </summary>
        [HttpPost("refund/{transactionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RefundTransaction(string transactionId, CancellationToken cancellationToken)
        {
            await _monetizationService.RefundTransactionAsync(transactionId, cancellationToken);
            return SuccessResp.Ok(new { message = $"Đã thực hiện lệnh hoàn tiền thành công cho giao dịch '{transactionId}'." });
        }
    }
}
