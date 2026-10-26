using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;

public sealed class PaymentHandler(ISaleRepository repository, IVehicleManagementService vehicleManagementService) :
    IRequestHandler<PaymentPaidCommand, Response>,
    IRequestHandler<PaymentCanceledCommand, Response>
{
    public async Task<Response> Handle(PaymentPaidCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetByPaymentCodeAsync(request.PaymentCode, cancellationToken);
        if (sale is null) return ResponseFactory.Fail($"Sale with payment code {request.PaymentCode} not found", HttpStatusCode.NotFound);

        var vehicleResponse = await vehicleManagementService.SellVehicleAsync(sale.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return ResponseFactory.Fail(vehicleResponse.Reasons[0].Message, vehicleResponse.StatusCode);

        await repository.UpdateAsync(sale.Paid(), cancellationToken);
        return ResponseFactory.Ok();
    }

    public async Task<Response> Handle(PaymentCanceledCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetByPaymentCodeAsync(request.PaymentCode, cancellationToken);
        if (sale is null) return ResponseFactory.Fail($"Sale with payment code {request.PaymentCode} not found", HttpStatusCode.NotFound);

        var vehicleResponse = await vehicleManagementService.ReleaseVehicleAsync(sale.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return ResponseFactory.Fail(vehicleResponse.Reasons[0].Message, vehicleResponse.StatusCode);

        await repository.UpdateAsync(sale.Canceled(), cancellationToken);
        return ResponseFactory.Ok();
    }
}
