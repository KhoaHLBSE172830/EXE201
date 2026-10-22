using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimo.Application.DTOs;
using Mimo.Application.DTOs.Storage;
using Mimo.Application.Middlewares;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.API.Controllers
{
    [ApiController]
    [Route("api/v1/media")]
    [Produces("application/json")]
    public class MediaController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IUserService _userService;

        public MediaController(IStorageService storageService, IUserService userService)
        {
            _storageService = storageService;
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

        private void ValidateImageFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Vui lòng chọn file để tải lên.");
            }

            if (file.Length > StorageConstants.MAX_IMAGE_SIZE_BYTES)
            {
                throw new BadRequestException("Dung lượng file vượt quá giới hạn cho phép (tối đa 10MB).");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!StorageConstants.ALLOWED_IMAGE_EXTENSIONS.Contains(extension))
            {
                throw new BadRequestException($"Định dạng file không được hỗ trợ. Các định dạng cho phép: {string.Join(", ", StorageConstants.ALLOWED_IMAGE_EXTENSIONS)}");
            }

            if (!StorageConstants.ALLOWED_IMAGE_MIME_TYPES.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new BadRequestException("Loại MIME của file không hợp lệ.");
            }
        }

        /// <summary>
        /// Tải lên ảnh đại diện cá nhân và tự động cập nhật vào hồ sơ người dùng
        /// </summary>
        [HttpPost("upload/avatar")]
        [Protected]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FileUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
        {
            ValidateImageFile(file);
            var userId = GetCurrentUserId();

            using var stream = file.OpenReadStream();
            var uploadResult = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder: $"{StorageConstants.Folders.AVATARS}/{userId}",
                cancellationToken: cancellationToken);

            // Cập nhật AvatarUrl trong database cho người dùng
            await _userService.UpdateAvatarAsync(userId, uploadResult.Url);

            return SuccessResp.Ok(uploadResult);
        }

        /// <summary>
        /// Tải lên hình ảnh / GIF minh họa tình huống câu hỏi (Dành riêng cho Curator và Admin)
        /// </summary>
        [HttpPost("upload/question-image")]
        [RequireRoles(UserRoleEnum.Admin, UserRoleEnum.Curator)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FileUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadQuestionImage(IFormFile file, CancellationToken cancellationToken)
        {
            ValidateImageFile(file);

            using var stream = file.OpenReadStream();
            var uploadResult = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder: StorageConstants.Folders.QUESTIONS,
                cancellationToken: cancellationToken);

            return SuccessResp.Ok(uploadResult);
        }

        /// <summary>
        /// Tải lên tệp đa dụng (Tài nguyên, biểu tượng Domain, huy hiệu...)
        /// </summary>
        [HttpPost("upload")]
        [Protected]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(FileUploadResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadGeneral(IFormFile file, [FromQuery] string? folder, CancellationToken cancellationToken)
        {
            ValidateImageFile(file);

            var targetFolder = string.IsNullOrWhiteSpace(folder) ? StorageConstants.Folders.GENERAL : folder.Trim();

            using var stream = file.OpenReadStream();
            var uploadResult = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder: targetFolder,
                cancellationToken: cancellationToken);

            return SuccessResp.Ok(uploadResult);
        }

        /// <summary>
        /// Xóa file khỏi Supabase Storage
        /// </summary>
        [HttpDelete]
        [Protected]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                throw new BadRequestException("Đường dẫn file cần xóa không được để trống.");
            }

            var deleted = await _storageService.DeleteFileAsync(fileUrl, cancellationToken);
            if (!deleted)
            {
                throw new BadRequestException("Không thể xóa file hoặc file không tồn tại.");
            }

            return SuccessResp.Ok(new { message = "Xóa file thành công." });
        }
    }
}
