using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.Filters;
using Mimo.Application.Services;

namespace Mimo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        
        // Register all FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<ValidationFilter>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<Helper.IQuestionGrader, Helper.QuestionGrader>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<IGameplayService, GameplayService>();
        services.AddScoped<IPracticeService, PracticeService>();
        services.AddScoped<ICurriculumService, CurriculumService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IQuestService, QuestService>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IHandbookService, HandbookService>();
        services.AddScoped<IClassMembershipService, ClassMembershipService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<ICuratorContentService, CuratorContentService>();
        services.AddScoped<IAdminContentService, AdminContentService>();
        return services;
    }
}

