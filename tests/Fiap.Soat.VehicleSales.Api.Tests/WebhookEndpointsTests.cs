using Fiap.Soat.VehicleSales.Api.Endpoints;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class WebhookEndpointsTests
{
    private readonly Mock<IWebhookController> _controller = new();

    [SetUp]
    public void SetUp() => _controller.Reset();

    [TestCase("Paid", PaymentStatus.Paid)]
    [TestCase("Canceled", PaymentStatus.Canceled)]
    public async Task ProcessPayment_DeserializesCommandAndCallsController(string status, PaymentStatus expectedStatus)
    {
        _controller
            .Setup(x => x.ProcessAsync(It.IsAny<PaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok());
        await using var server = await EndpointTestServer.StartAsync(_controller.Object, WebhookEndpoints.Map);

        var response = await server.Client.PostAsJsonAsync("api/v1/webhooks/payments", new { paymentCode = "code-123", status });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _controller.Verify(x => x.ProcessAsync(
            new PaymentCommand("code-123", expectedStatus),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
