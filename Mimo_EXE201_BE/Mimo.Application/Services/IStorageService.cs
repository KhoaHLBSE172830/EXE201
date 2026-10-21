using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Storage;

namespace Mimo.Application.Services
{
    public interface IStorageService
    {
        /// <summary>
        /// Tải file lên Supabase Cloud Storage
        /// </summary>
        /// <param name="fileStream">Stream nội dung file</param>
        /// <param name="fileName">Tên file gốc</param>
        /// <param name="contentType">MIME type (ví dụ: image/png, image/jpeg)</param>
        /// <param name="folder">Thư mục phân loại (avatars, questions, domains...)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Thông tin file đã tải lên và đường dẫn URL công khai</returns>
        Task<FileUploadResultDto> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "general",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa file khỏi Supabase Cloud Storage
        /// </summary>
        /// <param name="fileUrlOrPath">URL công khai hoặc đường dẫn tương đối trong bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteFileAsync(
            string fileUrlOrPath,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy URL công khai từ đường dẫn file trong bucket
        /// </summary>
        /// <param name="filePath">Đường dẫn trong bucket (ví dụ: avatars/sample.png)</param>
        /// <returns>URL công khai truy cập trực tiếp</returns>
        string GetPublicUrl(string filePath);
    }
}
