using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;

public sealed class ListVehicleHandler(IVehicleRepository repository) :
    IRequestHandler<GetAllAvailableVehiclesQuery, Response<IReadOnlyList<VehicleDetail>>>,
    IRequestHandler<GetAllSoldVehiclesQuery, Response<IReadOnlyList<VehicleDetail>>>
{
    private static readonly VehicleStatus[] AvailableStatuses = [VehicleStatus.Available];
    private static readonly VehicleStatus[] SoldStatuses = [VehicleStatus.Sold, VehicleStatus.Delivered];

    public Task<Response<IReadOnlyList<VehicleDetail>>> Handle(GetAllAvailableVehiclesQuery request, CancellationToken cancellationToken) =>
        ListAsync(AvailableStatuses, cancellationToken);

    public Task<Response<IReadOnlyList<VehicleDetail>>> Handle(GetAllSoldVehiclesQuery request, CancellationToken cancellationToken) =>
        ListAsync(SoldStatuses, cancellationToken);

    private async Task<Response<IReadOnlyList<VehicleDetail>>> ListAsync(IReadOnlyCollection<VehicleStatus> statuses, CancellationToken cancellationToken)
    {
        var vehicles = await repository.ListByStatusAsync(statuses, cancellationToken);
        return ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>(vehicles.Select(vehicle => vehicle.ToDetail()).ToList());
    }
}
