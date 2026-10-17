using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.Auth;

namespace Mimo.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserProfileDto> GetCurrentUserAsync(Guid userId);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
