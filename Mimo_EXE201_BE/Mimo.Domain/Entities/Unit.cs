using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("Units")]
    public class Unit : BaseEntity
    {
        [Required]
        public Guid DomainId { get; set; }

        [ForeignKey("DomainId")]
        public virtual Domain Domain { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Order { get; set; } = 0;

        public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();
    }
}
