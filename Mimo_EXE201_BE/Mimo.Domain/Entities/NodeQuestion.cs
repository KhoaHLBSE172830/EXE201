using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mimo.DomainLayer.Entities
{
    [Table("NodeQuestions")]
    public class NodeQuestion
    {
        public Guid NodeId { get; set; }

        [ForeignKey("NodeId")]
        public virtual Node Node { get; set; } = null!;

        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;

        public int Order { get; set; } = 0;
    }
}
