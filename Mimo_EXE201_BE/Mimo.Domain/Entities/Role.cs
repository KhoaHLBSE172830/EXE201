using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Roles")]
    public class Role : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Description { get; set; }

        public RoleStatusEnum Status { get; set; } = RoleStatusEnum.Active;

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
