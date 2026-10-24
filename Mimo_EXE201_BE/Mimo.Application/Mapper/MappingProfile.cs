using System;
using System.Text.Json;
using AutoMapper;
using Mimo.Application.DTOs.Auth;
using Mimo.Application.DTOs.Curriculum;
using Mimo.Application.DTOs.Gameplay;
using Mimo.Application.DTOs.Practice;
using Mimo.Application.DTOs.User;
using Mimo.DomainLayer.Entities;
using Mimo.DomainLayer.Helper;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ConfigureUserAndAuthMappings();
            ConfigureGameplayAndQuestionMappings();
            ConfigureCurriculumMappings();
        }

        /// <summary>
        /// Cấu hình ánh xạ giữa User / UserProfile / UserStats và các DTO tài khoản
        /// </summary>
        private void ConfigureUserAndAuthMappings()
        {
            // User -> UserProfileDto
            CreateMap<User, UserProfileDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => 
                    s.Profile != null && !string.IsNullOrWhiteSpace(s.Profile.FullName) 
                        ? s.Profile.FullName 
                        : s.Name))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.Phone))
                .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => 
                    s.Profile != null && !string.IsNullOrWhiteSpace(s.Profile.AvatarUrl) 
                        ? s.Profile.AvatarUrl 
                        : s.Avatar))
                .ForMember(d => d.RoleId, opt => opt.MapFrom(s => s.RoleId))
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role != null ? s.Role.RoleName : "Student"))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.OrganizationId, opt => opt.MapFrom(s => s.OrganizationId))
                .ForMember(d => d.OrganizationName, opt => opt.MapFrom(s => s.Organization != null ? s.Organization.Name : null))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt));

            // User -> UserDetailDto (Đầy đủ thông tin game hóa: XP, Level, Tim, Streak)
            CreateMap<User, UserDetailDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => 
                    s.Profile != null && !string.IsNullOrWhiteSpace(s.Profile.FullName) 
                        ? s.Profile.FullName 
                        : s.Name))
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.UserName))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
                .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.Phone))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
                .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => 
                    s.Profile != null && !string.IsNullOrWhiteSpace(s.Profile.AvatarUrl) 
                        ? s.Profile.AvatarUrl 
                        : s.Avatar))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.AuthType, opt => opt.MapFrom(s => s.AuthType ?? "Local"))
                .ForMember(d => d.RoleId, opt => opt.MapFrom(s => s.RoleId))
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role != null ? s.Role.RoleName : "Student"))
                .ForMember(d => d.OrganizationId, opt => opt.MapFrom(s => s.OrganizationId))
                .ForMember(d => d.OrganizationName, opt => opt.MapFrom(s => s.Organization != null ? s.Organization.Name : null))
                .ForMember(d => d.Timezone, opt => opt.MapFrom(s => s.Timezone))
                .ForMember(d => d.TotalXp, opt => opt.MapFrom(s => s.TotalXp))
                // Dẫn xuất Level và tiến độ theo Section 5.3
                .ForMember(d => d.Level, opt => opt.MapFrom(s => LevelHelper.GetLevelInfo(s.TotalXp).Level))
                .ForMember(d => d.LevelTitle, opt => opt.MapFrom(s => LevelHelper.GetLevelInfo(s.TotalXp).Title))
                .ForMember(d => d.XpToNextLevel, opt => opt.MapFrom(s => LevelHelper.GetLevelInfo(s.TotalXp).XpToNextLevel))
                .ForMember(d => d.LevelProgressPercent, opt => opt.MapFrom(s => LevelHelper.GetLevelInfo(s.TotalXp).ProgressPercent))
                // Hearts & Regeneration theo Section 5.1
                .ForMember(d => d.Hearts, opt => opt.MapFrom(s => s.Stats != null ? s.Stats.Hearts : 5))
                .ForMember(d => d.MaxHearts, opt => opt.MapFrom(_ => 5))
                .ForMember(d => d.NextHeartInSeconds, opt => opt.MapFrom(s => 
                    s.Stats != null ? s.Stats.CalculateCurrentHearts(DateTime.UtcNow).nextHeartInSeconds : 0))
                .ForMember(d => d.IsPro, opt => opt.MapFrom(s => s.Stats != null && s.Stats.IsPro))
                .ForMember(d => d.ProExpiresAt, opt => opt.MapFrom(s => s.Stats != null ? s.Stats.ProExpiresAt : null))
                // Streak & Hoạt động
                .ForMember(d => d.Streak, opt => opt.MapFrom(s => s.Stats != null ? s.Stats.Streak : 0))
                .ForMember(d => d.StreakFreezeCount, opt => opt.MapFrom(s => s.Stats != null ? s.Stats.StreakFreezeCount : 0))
                .ForMember(d => d.LastActiveDate, opt => opt.MapFrom(s => s.Stats != null ? s.Stats.LastActiveDate : null))
                // Onboarding Profile
                .ForMember(d => d.DailyGoalMinutes, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.DailyGoalMinutes : 10))
                .ForMember(d => d.RiskSurveyResult, opt => opt.MapFrom(s => s.Profile != null ? s.Profile.RiskSurveyResult : null))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt));

            // RegisterRequestDto -> User
            CreateMap<RegisterRequestDto, User>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName.Trim()))
                .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Trim().ToLower()))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.Email.Trim().Split('@', StringSplitOptions.None)[0]))
                .ForMember(d => d.AuthType, opt => opt.MapFrom(_ => "Local"))
                .ForMember(d => d.Timezone, opt => opt.MapFrom(_ => "Asia/Ho_Chi_Minh"))
                .ForMember(d => d.TotalXp, opt => opt.MapFrom(_ => 0))
                .ForMember(d => d.Status, opt => opt.MapFrom(_ => UserStatusEnum.Active));

            // User -> RegisterResponseDto
            CreateMap<User, RegisterResponseDto>()
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.Phone))
                .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role != null ? s.Role.RoleName : "Student"))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt));

            // UpdateProfileDto -> User
            CreateMap<UpdateProfileDto, User>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName.Trim()))
                .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.PhoneNumber))
                .ForMember(d => d.Avatar, opt => opt.MapFrom(s => s.AvatarUrl))
                .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
                .ForMember(d => d.Timezone, opt => opt.Condition(s => !string.IsNullOrWhiteSpace(s.Timezone)));
        }

        /// <summary>
        /// Cấu hình ánh xạ Gameplay và Question.
        /// TUÂN THỦ CRITICAL WARNING 1: BẢO MẬT ĐÁP ÁN (ZERO LEAKAGE)
        /// TUYỆT ĐỐI KHÔNG MAP CorrectData VÀO DTO HIỂN THỊ CLIENT
        /// </summary>
        private void ConfigureGameplayAndQuestionMappings()
        {
            // Question -> QuestionDisplayDto (ZERO LEAKAGE: CorrectData và Explanation không có trong DTO này)
            CreateMap<Question, QuestionDisplayDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl))
                .ForMember(d => d.Payload, opt => opt.MapFrom(s => DeserializeJson(s.Payload)))
                .ForMember(d => d.Difficulty, opt => opt.MapFrom(s => s.Difficulty))
                .ForMember(d => d.Source, opt => opt.MapFrom(s => s.Source))
                .ForMember(d => d.Order, opt => opt.Ignore());

            // NodeQuestion -> QuestionDisplayDto
            CreateMap<NodeQuestion, QuestionDisplayDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.QuestionId))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Question.Type))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Question.Title))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Question.ImageUrl))
                .ForMember(d => d.Payload, opt => opt.MapFrom(s => DeserializeJson(s.Question.Payload)))
                .ForMember(d => d.Difficulty, opt => opt.MapFrom(s => s.Question.Difficulty))
                .ForMember(d => d.Source, opt => opt.MapFrom(s => s.Question.Source))
                .ForMember(d => d.Order, opt => opt.MapFrom(s => s.Order));

            // WrongQuestion -> WrongQuestionDto (ZERO LEAKAGE)
            CreateMap<WrongQuestion, WrongQuestionDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.QuestionId, opt => opt.MapFrom(s => s.QuestionId))
                .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Question != null ? s.Question.Type : "choice"))
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Question != null ? s.Question.Title : string.Empty))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Question != null ? s.Question.ImageUrl : null))
                .ForMember(d => d.Payload, opt => opt.MapFrom(s => s.Question != null ? DeserializeJson(s.Question.Payload) : null))
                .ForMember(d => d.TimesWrong, opt => opt.MapFrom(s => s.TimesWrong))
                .ForMember(d => d.LastAttemptAt, opt => opt.MapFrom(s => s.LastAttemptAt));

            // LessonSession -> CompleteSessionResponseDto
            CreateMap<LessonSession, CompleteSessionResponseDto>()
                .ForMember(d => d.SessionId, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.NodeId, opt => opt.MapFrom(s => s.NodeId))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.TotalXpEarned, opt => opt.MapFrom(s => s.TotalXpEarned))
                .ForMember(d => d.HeartsLost, opt => opt.MapFrom(s => s.HeartsLost))
                .ForMember(d => d.MaxComboStreak, opt => opt.MapFrom(s => s.MaxComboStreak))
                .ForMember(d => d.HeartsRemaining, opt => opt.Ignore())
                .ForMember(d => d.IsPerfectLesson, opt => opt.Ignore())
                .ForMember(d => d.PerfectBonusXp, opt => opt.Ignore())
                .ForMember(d => d.StreakInfo, opt => opt.Ignore())
                .ForMember(d => d.UnlockedNextNodeId, opt => opt.Ignore());
        }

        /// <summary>
        /// Cấu hình ánh xạ Chủ đề, Chặng và Bài học (Skill Tree)
        /// </summary>
        private void ConfigureCurriculumMappings()
        {
            CreateMap<Domain, DomainDto>()
                .ForMember(d => d.Units, opt => opt.MapFrom(s => s.Units));

            CreateMap<Unit, UnitDto>()
                .ForMember(d => d.Nodes, opt => opt.MapFrom(s => s.Nodes));

            CreateMap<Node, NodeDto>()
                .ForMember(d => d.UserProgressStatus, opt => opt.Ignore())
                .ForMember(d => d.CompletedAt, opt => opt.Ignore());
        }

        private static object? DeserializeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<object>(json);
            }
            catch
            {
                return json;
            }
        }
    }
}
