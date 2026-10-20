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
        return services;
    }
}

