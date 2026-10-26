using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;

public sealed class ListVehicleHandler(IVehicleManagementService vehicleManagementService) :
    IRequestHandler<GetAllAvailableVehiclesQuery, Response<IReadOnlyList<VehicleDetail>>>,
    IRequestHandler<GetAllSoldVehiclesQuery, Response<IReadOnlyList<VehicleDetail>>>
{
    public async Task<Response<IReadOnlyList<VehicleDetail>>> Handle(GetAllAvailableVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehiclesResponse = await vehicleManagementService.GetAllAvailableAsync(cancellationToken);
        return vehiclesResponse.IsSuccess ?
            ResponseFactory.Ok(vehiclesResponse.Data) :
            ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>(string.Join(", ", vehiclesResponse.Reasons));
    }

    public async Task<Response<IReadOnlyList<VehicleDetail>>> Handle(GetAllSoldVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehiclesResponse = await vehicleManagementService.GetAllSoldAsync(cancellationToken);
        return vehiclesResponse.IsSuccess ?
            ResponseFactory.Ok(vehiclesResponse.Data) :
            ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>(string.Join(", ", vehiclesResponse.Reasons));
    }
}

