using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public sealed class VehicleController(IMediator mediator) : IVehicleController
{
    public async Task<Response<IReadOnlyList<VehicleDetail>>> GetAllAvailableVehicles(CancellationToken cancellationToken)
    {
        var query = new GetAllAvailableVehiclesQuery();
        var response = await mediator.Send(query, cancellationToken);
        return response;
    }

    public async Task<Response<IReadOnlyList<VehicleDetail>>> GetAllSoldVehicles(CancellationToken cancellationToken)
    {
        var query = new GetAllSoldVehiclesQuery();
        var response = await mediator.Send(query, cancellationToken);
        return response;
    }
}
