using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Infrastructure.Services;

public sealed class PaymentGatewayService(
    ILogger<PaymentGatewayService> logger,
    IHttpClientFactory httpClientFactory,
    IPaymentGatewayRepository repository) : IPaymentGatewayService
{
    public async Task<Response> CreatePaymentAsync(string paymentCode, CancellationToken cancellationToken = default)
    {
        var gateway = await repository.GetRegisteredAsync(cancellationToken);
        if (gateway is null)
            return ResponseFactory.Fail("No payment gateway is registered", HttpStatusCode.ServiceUnavailable);

        try
        {
            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(gateway.BaseUrl);

            using var response = await client.PostAsJsonAsync(
                "/api/v1/payments",
                new CreatePaymentRequest(paymentCode),
                cancellationToken);

            if (response.IsSuccessStatusCode)
                return ResponseFactory.Ok(response.StatusCode);

            var message = $"Payment gateway returned {(int)response.StatusCode} ({response.ReasonPhrase})";
            logger.LogError(
                "Failed to create payment for {PaymentCode}. Status code: {StatusCode}",
                paymentCode,
                response.StatusCode);
            return ResponseFactory.Fail(message, HttpStatusCode.BadGateway);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "Payment gateway is unavailable while creating payment for {PaymentCode}",
                paymentCode);
            return ResponseFactory.Fail("Payment gateway is unavailable", HttpStatusCode.BadGateway);
        }
    }

    private sealed record CreatePaymentRequest(string PaymentCode);
}
