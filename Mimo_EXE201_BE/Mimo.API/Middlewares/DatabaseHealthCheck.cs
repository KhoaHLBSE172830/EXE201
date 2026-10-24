using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mimo.Infrastructure.Database;

namespace Mimo.API.Middlewares;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly MimoDbContext _context;

    public DatabaseHealthCheck(MimoDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Kết nối PostgreSQL hoạt động bình thường.")
                : HealthCheckResult.Unhealthy("Không thể kết nối tới máy chủ PostgreSQL.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Lỗi kiểm tra trạng thái Database: {ex.Message}");
        }
    }
}
