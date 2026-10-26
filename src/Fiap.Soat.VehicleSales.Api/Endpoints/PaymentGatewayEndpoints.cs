using Fiap.Soat.VehicleSales.Api.Shared.Filters;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;

namespace Fiap.Soat.VehicleSales.Api.Endpoints;

public static class PaymentGatewayEndpoints
{
    public sealed record PaymentGatewayRequest(string Name, string BaseUrl);

    public static WebApplication Map(WebApplication app)
    {
        var gatewayApi = app
            .MapGroup("api/v1/payment-gateways")
            .AddEndpointFilter<ResponseFilter<PaymentGatewayDetail>>()
            .WithTags("Payment gateways");

        gatewayApi.MapPost("", ([FromServices] IPaymentGatewayController controller, [FromBody] PaymentGatewayRequest request, CancellationToken cancellationToken) =>
                controller.RegisterAsync(request.Name, request.BaseUrl, cancellationToken))
            .WithName("RegisterPaymentGateway");

        gatewayApi.MapGet("", ([FromServices] IPaymentGatewayController controller, CancellationToken cancellationToken) =>
                controller.GetAsync(cancellationToken))
            .WithName("GetPaymentGateway");

        gatewayApi.MapPut("{id:guid}", ([FromServices] IPaymentGatewayController controller, [FromRoute] Guid id, [FromBody] PaymentGatewayRequest request, CancellationToken cancellationToken) =>
                controller.UpdateAsync(id, request.Name, request.BaseUrl, cancellationToken))
            .WithName("UpdatePaymentGateway");

        return app;
    }
}
