using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Api.Shared.BackgroundServices;

public sealed class VehicleSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<VehicleSyncOptions> options,
    ILogger<VehicleSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.IntervalSeconds));
        try
        {
            do
            {
                await SyncAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Vehicle sync stopped");
        }
    }

    private async Task SyncAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var response = await mediator.Send(new SyncVehiclesCommand(), stoppingToken);
            if (response.IsSuccess)
            {
                logger.LogDebug("Vehicle sync received {Count} vehicles", response.Data);
                return;
            }

            logger.LogWarning("Vehicle sync failed: {Reason}", response.Reasons[0].Message);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Vehicle sync failed with an unexpected error");
        }
    }
}
