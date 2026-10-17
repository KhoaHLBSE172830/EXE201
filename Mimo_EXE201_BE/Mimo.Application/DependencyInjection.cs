using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Mimo.Application.Services;

namespace Mimo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
