using System;

namespace Mimo.DomainLayer.Helper
{
    public static class TimezoneHelper
    {
        public const string DEFAULT_TIMEZONE = "Asia/Ho_Chi_Minh";

        /// <summary>
        /// Lấy TimeZoneInfo chuẩn múi giờ Việt Nam (Asia/Ho_Chi_Minh - UTC+7) kèm cơ chế fallback an toàn
        /// </summary>
        public static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                try
                {
                    // Fallback cho Windows Server
                    return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                }
                catch
                {
                    return TimeZoneInfo.CreateCustomTimeZone("UTC+7", TimeSpan.FromHours(7), "UTC+7", "UTC+7");
                }
            }
        }

        /// <summary>
        /// Chuyển đổi một mốc thời gian UTC sang giờ địa phương Việt Nam
        /// </summary>
        public static DateTime ToVietnamTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, GetVietnamTimeZone());
        }

        /// <summary>
        /// Chuyển đổi giờ địa phương Việt Nam sang chuẩn UTC
        /// </summary>
        public static DateTime ToUtcTime(DateTime vietnamDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(vietnamDateTime, GetVietnamTimeZone());
        }
    }
}
