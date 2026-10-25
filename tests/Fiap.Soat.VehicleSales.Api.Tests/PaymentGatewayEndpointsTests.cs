using Fiap.Soat.VehicleSales.Api.Endpoints;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class PaymentGatewayEndpointsTests
{
    private readonly Mock<IPaymentGatewayController> _controller = new();
    private readonly PaymentGatewayDetail _gateway =
        new(Guid.NewGuid(), "Gateway", "https://gateway.com", DateTime.UtcNow, DateTime.UtcNow);

    [SetUp]
    public void SetUp() => _controller.Reset();

    [Test]
    public async Task RegisterGateway_PassesBodyToController()
    {
        _controller
            .Setup(x => x.RegisterAsync("Gateway", "https://gateway.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(_gateway, HttpStatusCode.Created));
        await using var server = await StartServer();

        var response = await server.Client.PostAsJsonAsync("api/v1/payment-gateways", new { name = "Gateway", baseUrl = "https://gateway.com" });

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("https://gateway.com"));
        });
        _controller.Verify(x => x.RegisterAsync("Gateway", "https://gateway.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterGateway_WhenControllerFails_ReturnsResponseStatusCode()
    {
        _controller
            .Setup(x => x.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<PaymentGatewayDetail>("A payment gateway is already registered", HttpStatusCode.Conflict));
        await using var server = await StartServer();

        var response = await server.Client.PostAsJsonAsync("api/v1/payment-gateways", new { name = "Gateway", baseUrl = "https://gateway.com" });

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("A payment gateway is already registered"));
        });
    }

    [Test]
    public async Task GetGateway_CallsController()
    {
        _controller
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(_gateway));
        await using var server = await StartServer();

        var response = await server.Client.GetAsync("api/v1/payment-gateways");

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Gateway"));
        });
        _controller.Verify(x => x.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateGateway_PassesRouteIdAndBodyToController()
    {
        var id = Guid.NewGuid();
        _controller
            .Setup(x => x.UpdateAsync(id, "New", "https://new.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(_gateway));
        await using var server = await StartServer();

        var response = await server.Client.PutAsJsonAsync($"api/v1/payment-gateways/{id}", new { name = "New", baseUrl = "https://new.com" });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _controller.Verify(x => x.UpdateAsync(id, "New", "https://new.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    private Task<EndpointTestServer> StartServer() =>
        EndpointTestServer.StartAsync(_controller.Object, PaymentGatewayEndpoints.Map);
}
