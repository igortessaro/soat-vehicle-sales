using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;

public interface IVehicleManagementService
{
    Task<Response<VehicleDetail>> ReserveVehicleAsync(string vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> ReleaseVehicleAsync(string vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> SellVehicleAsync(string vehicleId, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<VehicleDetail>>> ListVehiclesAsync(ListVehicleQuery query, CancellationToken cancellationToken = default);
}

public record ListVehicleQuery(string? LicensePlate, string? Brand, string? Model, int? Year, string? Color, string? Status);
