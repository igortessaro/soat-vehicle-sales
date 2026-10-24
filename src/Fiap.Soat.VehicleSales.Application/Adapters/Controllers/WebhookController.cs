using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public sealed class WebhookController(IMediator mediator) : IWebhookController
{
    public async Task<Response> ProcessAsync(PaymentCommand command, CancellationToken cancellationToken)
    {
        var response = command.Status switch
        {
            PaymentStatus.Paid => await mediator.Send(new PaymentPaidCommand(command.PaymentCode), cancellationToken),
            PaymentStatus.Canceled => await mediator.Send(new PaymentCanceledCommand(command.PaymentCode), cancellationToken)
        };
        return response;
    }
}
