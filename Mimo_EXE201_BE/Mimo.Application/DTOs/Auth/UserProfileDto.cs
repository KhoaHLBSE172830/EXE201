using System;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.DTOs.Auth
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRoleEnum Role { get; set; }
        public string RoleName => Role.ToString();
        public UserStatusEnum Status { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? OrganizationName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
