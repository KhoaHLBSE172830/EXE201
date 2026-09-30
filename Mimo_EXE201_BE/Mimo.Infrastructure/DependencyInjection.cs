using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.Repositories;
using Mimo.Application.Services;
using Mimo.Infrastructure.Core.Cache;
using Mimo.Infrastructure.Core.Crypto;
using Mimo.Infrastructure.Core.JWT;
using Mimo.Infrastructure.Core.Mail;
using Mimo.Infrastructure.Database;
using Mimo.Infrastructure.Repository;
using StackExchange.Redis;

namespace Mimo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Context (PostgreSQL)
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<MimoDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Core Infrastructure Services
        services.AddScoped<ICryptoService, CryptoService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IMailService, MailService>();

        // Redis & Cache Service
        var redisConn = configuration.GetConnectionString("Redis") 
            ?? configuration["Redis:ConnectionString"] 
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConn);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}
