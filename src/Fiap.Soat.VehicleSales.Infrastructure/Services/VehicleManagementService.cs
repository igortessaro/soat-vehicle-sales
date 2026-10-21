using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fiap.Soat.VehicleSales.Infrastructure.Services;

public sealed class VehicleManagementService(
    ILogger<VehicleManagementService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IVehicleManagementService
{
    public Task<Response<VehicleDetail>> ReserveVehicleAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Response<VehicleDetail>> ReleaseVehicleAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Response<VehicleDetail>> SellVehicleAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Response<IReadOnlyList<VehicleDetail>>> ListVehiclesAsync(ListVehicleQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
