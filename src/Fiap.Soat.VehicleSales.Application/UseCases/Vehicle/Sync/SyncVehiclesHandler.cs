using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;

public sealed class SyncVehiclesHandler(IVehicleRepository repository, IVehicleManagementService vehicleManagementService)
    : IRequestHandler<SyncVehiclesCommand, Response<int>>
{
    public async Task<Response<int>> Handle(SyncVehiclesCommand request, CancellationToken cancellationToken)
    {
        var updatedSince = await repository.GetLatestSourceUpdatedAtAsync(cancellationToken);
        var vehiclesResponse = await vehicleManagementService.GetUpdatedSinceAsync(updatedSince, cancellationToken);
        if (!vehiclesResponse.IsSuccess) return ResponseFactory.Fail<int>(vehiclesResponse.Reasons[0].Message, vehiclesResponse.StatusCode);

        foreach (var vehicle in vehiclesResponse.Data)
        {
            await repository.UpsertAsync(vehicle.ToSnapshot(), cancellationToken);
        }

        return ResponseFactory.Ok(vehiclesResponse.Data.Count);
    }
}
