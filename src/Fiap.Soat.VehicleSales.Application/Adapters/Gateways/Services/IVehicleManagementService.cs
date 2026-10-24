using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;

public interface IVehicleManagementService
{
    Task<Response<VehicleDetail>> ReserveVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> ReleaseVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> SellVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<VehicleDetail>> GetAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<VehicleDetail>>> GetAllAvailableAsync(CancellationToken cancellationToken = default);
    Task<Response<IReadOnlyList<VehicleDetail>>> GetAllSoldAsync(CancellationToken cancellationToken = default);
}

public record ListVehicleQuery(string? LicensePlate, string? Brand, string? Model, int? Year, string? Color, string? Status);
