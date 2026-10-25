using Fiap.Soat.VehicleSales.Api.Endpoints;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class SaleEndpointsTests
{
    private readonly Mock<ISaleController> _controller = new();

    [SetUp]
    public void SetUp() => _controller.Reset();

    [Test]
    public async Task CreateSale_WhenControllerSucceeds_ReturnsOkWithSale()
    {
        var vehicleId = Guid.NewGuid();
        var sale = CreateSaleDetail(vehicleId);
        _controller
            .Setup(x => x.CreateAsync(It.IsAny<CreateSaleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(sale, HttpStatusCode.Created));
        await using var server = await StartServer();

        var response = await server.Client.PostAsJsonAsync("api/v1/sales", new { vehicleId, buyerDocument = "52998224725" });

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain(sale.PaymentCode));
        });
        _controller.Verify(x => x.CreateAsync(
            It.Is<CreateSaleCommand>(c => c.VehicleId == vehicleId && c.BuyerDocument == "52998224725"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateSale_WhenControllerFails_ReturnsResponseStatusCodeAndErrors()
    {
        _controller
            .Setup(x => x.CreateAsync(It.IsAny<CreateSaleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<SaleDetail>("Vehicle is not available", HttpStatusCode.Conflict));
        await using var server = await StartServer();

        var response = await server.Client.PostAsJsonAsync("api/v1/sales", new { vehicleId = Guid.NewGuid(), buyerDocument = "52998224725" });

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Vehicle is not available"));
        });
    }

    [Test]
    public async Task DeliverSale_PassesRouteIdToController()
    {
        var id = Guid.NewGuid();
        _controller
            .Setup(x => x.DeliverAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(CreateSaleDetail(Guid.NewGuid())));
        await using var server = await StartServer();

        var response = await server.Client.PutAsync($"api/v1/sales/{id}/deliver", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _controller.Verify(x => x.DeliverAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeliverSale_WhenIdIsNotGuid_ReturnsNotFound()
    {
        await using var server = await StartServer();

        var response = await server.Client.PutAsync("api/v1/sales/not-a-guid/deliver", null);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        _controller.VerifyNoOtherCalls();
    }

    private Task<EndpointTestServer> StartServer() =>
        EndpointTestServer.StartAsync(_controller.Object, SaleEndpoints.Map);

    private static SaleDetail CreateSaleDetail(Guid vehicleId) =>
        new(Guid.NewGuid(), vehicleId, "52998224725", SaleStatus.PendingPayment, Guid.NewGuid().ToString("N"),
            null, null, DateTime.UtcNow, DateTime.UtcNow);
}
