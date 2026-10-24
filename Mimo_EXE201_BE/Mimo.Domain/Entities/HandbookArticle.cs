using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("HandbookArticles")]
    public class HandbookArticle
    {
        [Key]
        [MaxLength(80)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Pillar { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Badge { get; set; } = "Kỹ năng sống";

        [MaxLength(20)]
        public string ReadTime { get; set; } = "3 phút";

        /// <summary>
        /// JSON array of emergency dials, e.g. [{"name":"Cấp cứu","dial":"115"}]
        /// </summary>
        [Column(TypeName = "jsonb")]
        public string? EmergencyDials { get; set; }

        [Required]
        public string KeyProtocol { get; set; } = string.Empty;

        [Required]
        public string LegalBasis { get; set; } = string.Empty;

        [Required]
        public string ContentMarkdown { get; set; } = string.Empty;

        public int Order { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
