namespace Mimo.Application.DTOs.Storage
{
    public class FileUploadResultDto
    {
        public string Url { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
    }
}
