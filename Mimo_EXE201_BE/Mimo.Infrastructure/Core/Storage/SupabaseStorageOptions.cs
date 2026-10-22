namespace Mimo.Infrastructure.Core.Storage
{
    public class SupabaseStorageOptions
    {
        public const string SectionName = "Supabase";

        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = "mimo-assets";
    }
}
