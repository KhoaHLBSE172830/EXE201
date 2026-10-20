using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Nodes")]
    public class Node : BaseEntity
    {
        [Required]
        public Guid UnitId { get; set; }

        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Loại node: lesson (bài thường) | chest (rương) | boss (thử thách chặng)
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = "lesson";

        public int Order { get; set; } = 0;

        /// <summary>
        /// Điều kiện mở khóa lưu dưới dạng JSON (vd: { "prerequisite_node_id": "uuid" })
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? UnlockRule { get; set; }

        public int XpReward { get; set; } = 10;

        /// <summary>
        /// Trạng thái phê duyệt: Draft, PendingApproval, Published, Archived
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public virtual ICollection<NodeQuestion> NodeQuestions { get; set; } = new List<NodeQuestion>();
        public virtual ICollection<UserNodeProgress> UserNodeProgresses { get; set; } = new List<UserNodeProgress>();
        public virtual ICollection<LessonSession> LessonSessions { get; set; } = new List<LessonSession>();
    }
}
