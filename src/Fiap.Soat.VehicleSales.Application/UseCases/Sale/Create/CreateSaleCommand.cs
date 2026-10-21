using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

public record CreateSaleCommand(string VehicleId, string BuyerDocument, string PaymentCode)
    : IRequest<Response<VehicleSales.Domain.Entities.Sale>>;
