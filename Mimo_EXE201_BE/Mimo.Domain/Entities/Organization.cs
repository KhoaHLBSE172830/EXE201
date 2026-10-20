using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Organizations")]
    public class Organization : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        public int SeatCount { get; set; } = 50;

        public DateTime? ExpirationDate { get; set; }

        public OrganizationStatusEnum Status { get; set; } = OrganizationStatusEnum.Active;

        public SubscriptionTierEnum SubscriptionTier { get; set; } = SubscriptionTierEnum.Free;

        [NotMapped]
        public string OrgName
        {
            get => Name;
            set => Name = value;
        }

        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
