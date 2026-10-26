using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Infrastructure.Repositories;

public sealed class VehicleRepository(AppDbContext appDbContext) : Repository<Vehicle>(appDbContext), IVehicleRepository
{
    public async Task<Vehicle> UpsertAsync(VehicleSnapshot snapshot, CancellationToken cancellationToken)
    {
        var vehicle = await GetByIdAsync(snapshot.Id, cancellationToken);
        try
        {
            if (vehicle is null) return await AddAsync(new Vehicle(snapshot), cancellationToken);

            return vehicle.ApplySnapshot(snapshot) ? await UpdateAsync(vehicle, cancellationToken) : vehicle;
        }
        catch
        {
            DetachVehicle(snapshot.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Vehicle>> ListByStatusAsync(IReadOnlyCollection<VehicleStatus> statuses, CancellationToken cancellationToken) =>
        await Query()
            .Where(x => Enumerable.Contains(statuses, x.Status))
            .OrderBy(x => x.Price)
            .ToListAsync(cancellationToken);

    public Task<DateTime?> GetLatestSourceUpdatedAtAsync(CancellationToken cancellationToken) =>
        Query().MaxAsync(x => (DateTime?) x.SourceUpdatedAt, cancellationToken);

    private void DetachVehicle(Guid id)
    {
        foreach (var entry in appDbContext.ChangeTracker.Entries<Vehicle>().Where(entry => entry.Entity.Id == id))
        {
            entry.State = EntityState.Detached;
        }
    }
}
