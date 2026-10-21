using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;

namespace Fiap.Soat.VehicleSales.Api.Shared.HealthChecks;

public sealed class DetailedHealthCheck : IHealthCheck
{
    private static readonly string Version = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion ?? "1.0.0";

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            { "version", Version },
            { "timestamp", DateTimeOffset.UtcNow.ToString("o") },
            { "uptime", Environment.TickCount64 / 1000.0 }
        };

        try
        {
            var memoryUsed = GC.GetTotalMemory(false);
            var memoryUsedMB = memoryUsed / 1024.0 / 1024.0;
            data["memoryUsedMB"] = Math.Round(memoryUsedMB, 2);

            data["gen0Collections"] = GC.CollectionCount(0);
            data["gen1Collections"] = GC.CollectionCount(1);
            data["gen2Collections"] = GC.CollectionCount(2);

            return Task.FromResult(HealthCheckResult.Healthy("API is healthy", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("API is unhealthy", ex, data));
        }
    }
}
