using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Questions")]
    public class Question : BaseEntity
    {
        /// <summary>
        /// Dạng câu hỏi: choice (chọn quyết định), sequence (sắp xếp), red_flag (tìm điểm bất thường)
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Type { get; set; } = "choice";

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        /// <summary>
        /// Dữ liệu câu hỏi hiển thị cho client (JSON) — TUYỆT ĐỐI KHÔNG CHỨA ĐÁP ÁN (ZERO LEAKAGE)
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string Payload { get; set; } = "{}";

        /// <summary>
        /// Đáp án đúng (JSON) để backend đối chiếu chấm điểm
        /// </summary>
        [Required]
        [Column(TypeName = "jsonb")]
        public string CorrectData { get; set; } = "{}";

        /// <summary>
        /// Lời giải thích chuyên môn y tế/PCCC
        /// </summary>
        [Required]
        public string Explanation { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Difficulty { get; set; } = "Medium";

        [MaxLength(255)]
        public string? Source { get; set; }

        [MaxLength(150)]
        public string? ReviewedBy { get; set; }

        public Guid? CuratorId { get; set; }

        [ForeignKey("CuratorId")]
        public virtual User? Curator { get; set; }

        /// <summary>
        /// Draft, PendingApproval, Published, Rejected
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";

        public virtual ICollection<NodeQuestion> NodeQuestions { get; set; } = new List<NodeQuestion>();
        public virtual ICollection<SessionAnswer> SessionAnswers { get; set; } = new List<SessionAnswer>();
        public virtual ICollection<WrongQuestion> WrongQuestions { get; set; } = new List<WrongQuestion>();
    }
}
