using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Admin
{
    /// <summary>
    /// DTO chỉ số sống còn của hệ thống (North Star Metrics & System Health)
    /// </summary>
    public class AdminAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int Dau { get; set; }
        public int Mau { get; set; }
        public double Stickiness { get; set; } // DAU / MAU (%)
        public string DauGrowth { get; set; } = "+14.2%";
        public double D7Retention { get; set; } = 42.8;
        public double D30Retention { get; set; } = 21.4;
        public long LivesSaved { get; set; }
        public long Mrr { get; set; } // Doanh thu định kỳ tháng (VNĐ)
        public string MrrGrowth { get; set; } = "+18.5%";
        public int CoursesPublished { get; set; }
        public int PendingApprovals { get; set; }
        public int UnresolvedTickets { get; set; }
    }

    /// <summary>
    /// DTO phân tích lỗ hổng kiến thức sinh tồn (Safety Skill Gap Heatmap)
    /// </summary>
    public class AdminSkillGapDto
    {
        public string Id { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CorrectRate { get; set; }
        public int WrongRate { get; set; }
        public string RiskLevel { get; set; } = "MEDIUM"; // HIGH | MEDIUM | LOW | SAFE
        public string CriticalFlaw { get; set; } = string.Empty;
        public string CorrectProtocol { get; set; } = string.Empty;
        public int TotalTests { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO yêu cầu phát cảnh báo kỹ năng khẩn cấp toàn quốc
    /// </summary>
    public class BroadcastAlertRequestDto
    {
        public string? TopicId { get; set; }
        public string CustomMessage { get; set; } = string.Empty;
    }
}
