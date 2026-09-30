using System;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.DTOs;

public class Payload
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public Guid SessionId { get; set; }
    public UserStatusEnum Status { get; set; }
    public string? Role { get; set; }
    public bool IsAdmin { get; set; }
}
