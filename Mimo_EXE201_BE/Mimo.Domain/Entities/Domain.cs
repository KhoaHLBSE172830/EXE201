using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Domains")]
    public class Domain : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        public string? IconUrl { get; set; }

        public int Order { get; set; } = 0;

        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}
