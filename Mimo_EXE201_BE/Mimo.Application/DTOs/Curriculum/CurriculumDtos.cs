using System;
using System.Collections.Generic;

namespace Mimo.Application.DTOs.Curriculum
{
    /// <summary>
    /// DTO tóm tắt danh sách chủ đề lớn (Domain) cho API GET /api/v1/domains
    /// </summary>
    public class DomainOverviewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int Order { get; set; }
        public string Status { get; set; } = "Active";
        public int TotalUnits { get; set; }
        public int TotalNodes { get; set; }
    }

    /// <summary>
    /// DTO chi tiết một chủ đề kèm các chặng kỹ năng
    /// </summary>
    public class DomainDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
        public int Order { get; set; }
        public string Status { get; set; } = "Active";
        public List<UnitDto> Units { get; set; } = new();
    }

    /// <summary>
    /// DTO chặng kỹ năng (Unit) cho API GET /api/v1/units/{id}
    /// </summary>
    public class UnitDto
    {
        public Guid Id { get; set; }
        public Guid DomainId { get; set; }
        public string? DomainName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Order { get; set; }
        public List<NodeDto> Nodes { get; set; } = new();
    }

    /// <summary>
    /// DTO bài học / nút trên cây kỹ năng (Node)
    /// </summary>
    public class NodeDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "lesson"; // lesson | chest | boss
        public int Order { get; set; }
        public int XpReward { get; set; } = 10;
        public string Status { get; set; } = "Draft";
        public string? UnlockRule { get; set; }
        
        /// <summary>
        /// Trạng thái tiến trình học tập của người dùng: locked | current | done
        /// </summary>
        public string UserProgressStatus { get; set; } = "locked";
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO Lộ trình học (Learning Path) hoàn chỉnh cho API GET /api/v1/path?domainId={id}
    /// </summary>
    public class LearningPathDto
    {
        public Guid DomainId { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string DomainSlug { get; set; } = string.Empty;
        public string? DomainIconUrl { get; set; }
        public int TotalNodes { get; set; }
        public int CompletedNodes { get; set; }
        public double ProgressPercent { get; set; }
        public List<UnitDto> Units { get; set; } = new();
    }
}
