using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Mappers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Copy;

public sealed class RefreshVehicleCopyHandler(IVehicleRepository repository, ILogger<RefreshVehicleCopyHandler> logger)
    : INotificationHandler<VehicleChangedNotification>
{
    public async Task Handle(VehicleChangedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await repository.UpsertAsync(notification.Vehicle.ToSnapshot(), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Could not refresh the local copy of vehicle {VehicleId}; the next sync will fix it", notification.Vehicle.Id);
        }
    }
}
