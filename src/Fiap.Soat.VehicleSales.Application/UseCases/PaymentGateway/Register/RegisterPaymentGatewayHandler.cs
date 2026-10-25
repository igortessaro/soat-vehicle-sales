using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Register;

public sealed class RegisterPaymentGatewayHandler(IPaymentGatewayRepository repository)
    : IRequestHandler<RegisterPaymentGatewayCommand, Response<VehicleSales.Domain.Entities.PaymentGateway>>
{
    public async Task<Response<VehicleSales.Domain.Entities.PaymentGateway>> Handle(RegisterPaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        var validationError = PaymentGatewayValidation.Validate(request.Name, request.BaseUrl);
        if (validationError is not null) return ResponseFactory.Fail<VehicleSales.Domain.Entities.PaymentGateway>(validationError);
        if (await repository.GetRegisteredAsync(cancellationToken) is not null)
            return ResponseFactory.Fail<VehicleSales.Domain.Entities.PaymentGateway>("A payment gateway is already registered", HttpStatusCode.Conflict);

        return ResponseFactory.Ok(await repository.AddAsync(new VehicleSales.Domain.Entities.PaymentGateway(request.Name.Trim(), request.BaseUrl), cancellationToken), HttpStatusCode.Created);
    }
}
