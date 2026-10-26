using Fiap.Soat.VehicleSales.Api.Shared.Filters;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Soat.VehicleSales.Api.Endpoints;

public static class WebhookEndpoints
{
    public static WebApplication Map(WebApplication app)
    {
        var webhookApi = app
            .MapGroup("api/v1/webhooks")
            .AddEndpointFilter<ResponseFilter<object>>()
            .WithTags("Webhook");

        webhookApi.MapPost("payments", ([FromServices] IWebhookController controller, [FromBody] PaymentCommand request, CancellationToken cancellationToken) =>
                controller.ProcessAsync(request, cancellationToken))
            .WithName("ProcessPaymentWebhook")
            .WithDescription("Process a payment by webhook request");

        return app;
    }
}
