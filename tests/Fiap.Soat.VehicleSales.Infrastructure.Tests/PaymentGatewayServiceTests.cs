using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class PaymentGatewayServiceTests
{
    private readonly Mock<IPaymentGatewayRepository> _repository = new();

    [SetUp]
    public void SetUp() => _repository.Reset();

    [Test]
    public async Task CreatePaymentAsync_WhenNoGatewayIsRegistered_ReturnsServiceUnavailable()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK);
        _repository.Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>())).ReturnsAsync((PaymentGateway?) null);

        var response = await CreateService(handler).CreatePaymentAsync("code");

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("No payment gateway is registered"));
            Assert.That(handler.Requests, Is.Empty);
        });
    }

    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Created)]
    public async Task CreatePaymentAsync_WhenGatewayAccepts_PostsPaymentCodeAndReturnsSuccess(HttpStatusCode statusCode)
    {
        var handler = StubHttpMessageHandler.Returning(statusCode);
        SetupGateway();

        var response = await CreateService(handler).CreatePaymentAsync("code-123");

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(statusCode));
            Assert.That(handler.Requests.Single().Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.Requests.Single().RequestUri, Is.EqualTo(new Uri("https://gateway.com/api/v1/payments")));
            Assert.That(handler.RequestBodies.Single(), Does.Contain("\"paymentCode\":\"code-123\""));
        });
    }

    [Test]
    public async Task CreatePaymentAsync_WhenGatewayRejects_ReturnsBadGateway()
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);
        SetupGateway();

        var response = await CreateService(handler).CreatePaymentAsync("code");

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Payment gateway returned 500 (Internal Server Error)"));
        });
    }

    [Test]
    public async Task CreatePaymentAsync_WhenGatewayIsUnreachable_ReturnsBadGateway()
    {
        var handler = StubHttpMessageHandler.Throwing(new HttpRequestException("Connection refused"));
        SetupGateway();

        var response = await CreateService(handler).CreatePaymentAsync("code");

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Payment gateway is unavailable"));
        });
    }

    private void SetupGateway() =>
        _repository
            .Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentGateway("Gateway", "https://gateway.com"));

    private PaymentGatewayService CreateService(StubHttpMessageHandler handler) =>
        new(NullLogger<PaymentGatewayService>.Instance,
            InfrastructureTestFixture.CreateHttpClientFactory(handler).Object,
            _repository.Object);
}
