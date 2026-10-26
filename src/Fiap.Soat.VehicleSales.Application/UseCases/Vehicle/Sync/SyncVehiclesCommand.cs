using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;

public record SyncVehiclesCommand : IRequest<Response<int>>;
