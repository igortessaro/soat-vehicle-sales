using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<Vehicle> UpsertAsync(VehicleSnapshot snapshot, CancellationToken cancellationToken);
    Task<IReadOnlyList<Vehicle>> ListByStatusAsync(IReadOnlyCollection<VehicleStatus> statuses, CancellationToken cancellationToken);
    Task<DateTime?> GetLatestSourceUpdatedAtAsync(CancellationToken cancellationToken);
}
