using System;
using Mimo.Application.DTOs;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string role, Guid sessionId, string email, UserStatusEnum status, int exp);
    Payload? ValidateToken(string token);
}
