using System;
using System.Threading.Tasks;
using Mimo.Application.DTOs.User;

namespace Mimo.Application.Services
{
    public interface IUserService
    {
        Task<UserDetailDto> GetMeAsync(Guid userId);
        Task<UserDetailDto> UpdateOnboardingAsync(Guid userId, UpdateOnboardingDto request);
        Task<UserDetailDto> UpdateProfileAsync(Guid userId, UpdateProfileDto request);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto request);
        Task<UserDetailDto> UpdateAvatarAsync(Guid userId, string avatarUrl);
        Task<Mimo.Application.DTOs.Gamification.StudyStatsResponseDto> GetStudyStatsAsync(Guid userId);
    }
}
