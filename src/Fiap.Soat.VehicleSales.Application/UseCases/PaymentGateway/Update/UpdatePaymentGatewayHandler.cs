using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Update;

public sealed class UpdatePaymentGatewayHandler(IPaymentGatewayRepository repository)
    : IRequestHandler<UpdatePaymentGatewayCommand, Response<VehicleSales.Domain.Entities.PaymentGateway>>
{
    public async Task<Response<VehicleSales.Domain.Entities.PaymentGateway>> Handle(UpdatePaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var validationError = PaymentGatewayValidation.Validate(request.Name, request.BaseUrl);
        if (validationError is not null) return ResponseFactory.Fail<VehicleSales.Domain.Entities.PaymentGateway>(validationError);

        var gateway = await repository.GetByIdAsync(request.Id, cancellationToken);
        return gateway is null
            ? ResponseFactory.Fail<VehicleSales.Domain.Entities.PaymentGateway>("Payment gateway not found", HttpStatusCode.NotFound)
            : ResponseFactory.Ok(await repository.UpdateAsync(gateway.Update(request.Name.Trim(), request.BaseUrl), cancellationToken));
    }
}
