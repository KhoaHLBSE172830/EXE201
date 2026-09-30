using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mimo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        return services;
    }
}
