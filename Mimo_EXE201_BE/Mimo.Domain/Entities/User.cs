using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.DomainLayer.Entities
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        public Guid? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public virtual Organization? Organization { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public string FullName
        {
            get => Name;
            set => Name = value;
        }

        [MaxLength(40)]
        public string? UserName { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        [NotMapped]
        public string PasswordHash
        {
            get => Password;
            set => Password = value;
        }

        [MaxLength(11)]
        public string? Phone { get; set; }

        [NotMapped]
        public string? PhoneNumber
        {
            get => Phone;
            set => Phone = value;
        }

        public DateTime? LastLogin { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public UserStatusEnum Status { get; set; } = UserStatusEnum.Active;

        [MaxLength(50)]
        public string? AuthType { get; set; } = "Local";

        [MaxLength(1000)]
        public string? Avatar { get; set; }

        [NotMapped]
        public string? AvatarUrl
        {
            get => Avatar;
            set => Avatar = value;
        }

        [MaxLength(255)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        [MaxLength(50)]
        public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

        public int TotalXp { get; set; } = 0;

        public virtual UserProfile? Profile { get; set; }
        public virtual UserStats? Stats { get; set; }

        public virtual ICollection<Course> InstructedCourses { get; set; } = new List<Course>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public virtual ICollection<UserNodeProgress> UserNodeProgresses { get; set; } = new List<UserNodeProgress>();
        public virtual ICollection<LessonSession> LessonSessions { get; set; } = new List<LessonSession>();
        public virtual ICollection<WrongQuestion> WrongQuestions { get; set; } = new List<WrongQuestion>();
        public virtual ICollection<XpEvent> XpEvents { get; set; } = new List<XpEvent>();
        public virtual ICollection<UserQuest> UserQuests { get; set; } = new List<UserQuest>();
        public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
        public virtual ICollection<ShopPurchase> ShopPurchases { get; set; } = new List<ShopPurchase>();
        public virtual ICollection<ClassMember> ClassMembers { get; set; } = new List<ClassMember>();
        public virtual ICollection<Class> EducatorClasses { get; set; } = new List<Class>();
        public virtual ICollection<AssignmentProgress> AssignmentProgresses { get; set; } = new List<AssignmentProgress>();
        public virtual ICollection<TeacherLicense> ClaimedTeacherLicenses { get; set; } = new List<TeacherLicense>();
    }
}
