using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Auth;

namespace Mimo.Application.Services
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<RegisterResponseDto> RegisterOrganizationAsync(OrganizationRegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> GoogleLoginAsync(GoogleAuthRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(Guid userId, Guid sessionId);
        Task<bool> ForceLogoutAsync(Guid userId);
    }
}
