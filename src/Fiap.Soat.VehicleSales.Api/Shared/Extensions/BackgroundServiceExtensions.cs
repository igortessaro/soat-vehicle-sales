using Fiap.Soat.VehicleSales.Api.Shared.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class BackgroundServiceExtensions
{
    public static IServiceCollection AddBackgroundServiceExtensions(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var section = configuration.GetSection(VehicleSyncOptions.SectionName);
        _ = serviceCollection.AddOptions<VehicleSyncOptions>()
            .Bind(section)
            .Validate(options => options.IntervalSeconds > 0, $"{VehicleSyncOptions.SectionName}:IntervalSeconds must be greater than zero")
            .ValidateOnStart();

        if (section.GetValue(nameof(VehicleSyncOptions.Enabled), defaultValue: true))
        {
            _ = serviceCollection.AddHostedService<VehicleSyncBackgroundService>();
        }

        return serviceCollection;
    }
}
