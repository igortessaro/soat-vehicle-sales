using Fiap.Soat.VehicleSales.Api.Endpoints;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class VehiclesEndpointsTests
{
    private readonly Mock<IVehicleController> _controller = new();

    [SetUp]
    public void SetUp() => _controller.Reset();

    [TestCase("available")]
    [TestCase("sold")]
    public async Task GetVehicles_WhenControllerSucceeds_ReturnsOkWithVehicles(string status)
    {
        IReadOnlyList<VehicleDetail> vehicles =
            [new(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", 100000m, status, DateTime.UtcNow, DateTime.UtcNow)];
        SetupController(status, ResponseFactory.Ok(vehicles));
        await using var server = await StartServer();

        var response = await server.Client.GetAsync($"api/v1/vehicles/{status}");

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Corolla"));
        });
    }

    [TestCase("available")]
    [TestCase("sold")]
    public async Task GetVehicles_WhenControllerFails_ReturnsResponseStatusCodeAndErrors(string status)
    {
        SetupController(status, ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>("Service unavailable", HttpStatusCode.BadGateway));
        await using var server = await StartServer();

        var response = await server.Client.GetAsync($"api/v1/vehicles/{status}");

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("Service unavailable"));
        });
    }

    private void SetupController(string status, Response<IReadOnlyList<VehicleDetail>> result)
    {
        if (status == "available")
            _controller.Setup(x => x.GetAllAvailableVehicles(It.IsAny<CancellationToken>())).ReturnsAsync(result);
        else
            _controller.Setup(x => x.GetAllSoldVehicles(It.IsAny<CancellationToken>())).ReturnsAsync(result);
    }

    private Task<EndpointTestServer> StartServer() =>
        EndpointTestServer.StartAsync(_controller.Object, VehiclesEndpoints.Map);
}
