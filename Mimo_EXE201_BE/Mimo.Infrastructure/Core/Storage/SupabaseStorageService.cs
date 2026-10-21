using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimo.Application.DTOs.Storage;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.Infrastructure.Core.Storage
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseStorageOptions _options;
        private readonly ILogger<SupabaseStorageService> _logger;

        public SupabaseStorageService(
            HttpClient httpClient,
            IOptions<SupabaseStorageOptions> options,
            ILogger<SupabaseStorageService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.Url))
            {
                throw new ArgumentException("Supabase URL chưa được cấu hình.", nameof(_options.Url));
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ArgumentException("Supabase ApiKey chưa được cấu hình.", nameof(_options.ApiKey));
            }
        }

        public async Task<FileUploadResultDto> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder = "general",
            CancellationToken cancellationToken = default)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new BadRequestException("Nội dung file tải lên không hợp lệ hoặc rỗng.");
            }

            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Tạo tên file ngẫu nhiên an toàn để chống trùng lặp và path traversal
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var sanitizedName = Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"[^a-zA-Z0-9_-]", "");
            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                sanitizedName = "file";
            }

            var uniqueFileName = $"{sanitizedName}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}{extension}";
            var sanitizedFolder = folder.Trim().Trim('/').Replace('\\', '/');
            var filePath = string.IsNullOrWhiteSpace(sanitizedFolder) ? uniqueFileName : $"{sanitizedFolder}/{uniqueFileName}";

            var requestUri = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.BucketName}/{filePath}";

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            request.Headers.Add("apikey", _options.ApiKey);
            request.Headers.Add("x-upsert", "true");

            using var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = streamContent;

            _logger.LogInformation("Đang tải file lên Supabase Storage: {FilePath}", filePath);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Lỗi khi tải file lên Supabase Storage: Status {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                throw new ServerFailureException($"Tải file lên Supabase Storage thất bại: {errorBody}");
            }

            var publicUrl = GetPublicUrl(filePath);

            return new FileUploadResultDto
            {
                Url = publicUrl,
                FilePath = filePath,
                FileName = uniqueFileName,
                ContentType = contentType,
                SizeBytes = fileStream.Length
            };
        }

        public async Task<bool> DeleteFileAsync(string fileUrlOrPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrPath))
            {
                return false;
            }

            var relativePath = ExtractRelativePath(fileUrlOrPath);
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var requestUri = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.BucketName}/{relativePath}";

            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
            request.Headers.Add("apikey", _options.ApiKey);

            _logger.LogInformation("Đang xóa file trên Supabase Storage: {RelativePath}", relativePath);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Không thể xóa file trên Supabase Storage ({RelativePath}): {StatusCode} {ErrorBody}", relativePath, response.StatusCode, errorBody);
                return false;
            }

            return true;
        }

        public string GetPublicUrl(string filePath)
        {
            var cleanPath = filePath.Trim().TrimStart('/');
            return $"{_options.Url.TrimEnd('/')}/storage/v1/object/public/{_options.BucketName}/{cleanPath}";
        }

        private string ExtractRelativePath(string fileUrlOrPath)
        {
            var raw = fileUrlOrPath.Trim();
            var publicPrefix = $"/storage/v1/object/public/{_options.BucketName}/";
            var directPrefix = $"/storage/v1/object/{_options.BucketName}/";

            var pubIndex = raw.IndexOf(publicPrefix, StringComparison.OrdinalIgnoreCase);
            if (pubIndex >= 0)
            {
                return raw.Substring(pubIndex + publicPrefix.Length);
            }

            var dirIndex = raw.IndexOf(directPrefix, StringComparison.OrdinalIgnoreCase);
            if (dirIndex >= 0)
            {
                return raw.Substring(dirIndex + directPrefix.Length);
            }

            return raw.TrimStart('/');
        }
    }
}
