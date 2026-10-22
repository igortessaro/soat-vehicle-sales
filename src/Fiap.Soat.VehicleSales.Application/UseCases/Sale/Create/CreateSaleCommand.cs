using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

public record CreateSaleCommand(Guid VehicleId, string BuyerDocument)
    : IRequest<Response<VehicleSales.Domain.Entities.Sale>>;
