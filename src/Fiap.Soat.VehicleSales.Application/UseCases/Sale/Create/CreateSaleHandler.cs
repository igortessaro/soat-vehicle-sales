using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

public sealed class CreateSaleHandler(
    ISaleRepository repository,
    IVehicleManagementService vehicleManagementService,
    IPaymentGatewayService paymentGatewayService)
    : IRequestHandler<CreateSaleCommand, Response<VehicleSales.Domain.Entities.Sale>>
{
    public async Task<Response<VehicleSales.Domain.Entities.Sale>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.VehicleId == Guid.Empty) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("VehicleId is required");
        if (string.IsNullOrWhiteSpace(request.BuyerDocument)) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("BuyerDocument is required");

        var vehicleResponse = await vehicleManagementService.GetAsync(request.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>(vehicleResponse.Reasons[0].Message);

        var entity = request.ToDomainEntity();
        if (!entity.BuyerDocument.IsValid())
            return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>($"BuyerDocument {entity.BuyerDocument.Value} is invalid");

        var reserveResponse = await vehicleManagementService.ReserveVehicleAsync(request.VehicleId, cancellationToken);
        if (!reserveResponse.IsSuccess)
            return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>(reserveResponse.Reasons[0].Message, reserveResponse.StatusCode);

        var sale = await repository.AddAsync(entity, cancellationToken);

        var paymentResponse = await paymentGatewayService.CreatePaymentAsync(sale.PaymentCode, cancellationToken);
        if (!paymentResponse.IsSuccess)
            return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>(
                paymentResponse.Reasons[0].Message,
                paymentResponse.StatusCode);

        var updatedSale = await repository.FindSingleAsync(item => item.Id == sale.Id, cancellationToken);
        return updatedSale is null
            ? ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("Sale could not be loaded after payment processing")
            : ResponseFactory.Ok(updatedSale);
    }
}
