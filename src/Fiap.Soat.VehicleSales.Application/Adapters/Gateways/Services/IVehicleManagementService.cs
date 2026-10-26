using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;

public interface IVehicleManagementService
{
    Task<Response<VehicleDetail>> ReserveVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> ReleaseVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> SellVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> DeliverVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> GetAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<VehicleDetail>>> GetUpdatedSinceAsync(DateTime? updatedSince, CancellationToken cancellationToken = default);
}
