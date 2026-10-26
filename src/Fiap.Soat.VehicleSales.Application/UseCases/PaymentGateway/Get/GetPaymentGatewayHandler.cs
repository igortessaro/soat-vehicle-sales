using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Get;

public sealed class GetPaymentGatewayHandler(IPaymentGatewayRepository repository)
    : IRequestHandler<GetPaymentGatewayQuery, Response<VehicleSales.Domain.Entities.PaymentGateway>>
{
    public async Task<Response<VehicleSales.Domain.Entities.PaymentGateway>> Handle(GetPaymentGatewayQuery request, CancellationToken cancellationToken)
    {
        var gateway = await repository.GetRegisteredAsync(cancellationToken);
        return gateway is null
            ? ResponseFactory.Fail<VehicleSales.Domain.Entities.PaymentGateway>("No payment gateway is registered", HttpStatusCode.NotFound)
            : ResponseFactory.Ok(gateway);
    }
}
