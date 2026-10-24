using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;

public record GetAllAvailableVehiclesQuery() : IRequest<Response<IReadOnlyList<VehicleDetail>>>;
