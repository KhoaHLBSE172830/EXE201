using System;
using System.Linq;

namespace Mimo.DomainLayer.Constants
{
    public static class StorageConstants
    {
        public const string DEFAULT_BUCKET = "mimo-assets";
        public const long MAX_IMAGE_SIZE_BYTES = 10 * 1024 * 1024; // 10MB

        public static readonly string[] ALLOWED_IMAGE_EXTENSIONS = 
        { 
            ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" 
        };

        public static readonly string[] ALLOWED_IMAGE_MIME_TYPES = 
        { 
            "image/jpeg", 
            "image/png", 
            "image/webp", 
            "image/gif", 
            "image/svg+xml" 
        };

        public static class Folders
        {
            public const string AVATARS = "avatars";
            public const string QUESTIONS = "questions";
            public const string DOMAINS = "domains";
            public const string BADGES = "badges";
            public const string GENERAL = "general";
        }
    }
}
