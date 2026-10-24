using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public interface IVehicleController
{
    Task<Response<IReadOnlyList<VehicleDetail>>> GetAllAvailableVehicles(CancellationToken cancellationToken);
    Task<Response<IReadOnlyList<VehicleDetail>>> GetAllSoldVehicles(CancellationToken cancellationToken);
}
