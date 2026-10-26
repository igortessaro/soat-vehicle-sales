using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Integration.Tests.Factories;
using System.Net;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Vehicles;

public sealed class ListVehiclesTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/vehicles";

    [TestCase("available", "Available")]
    [TestCase("sold", "Sold")]
    public async Task Vehicles_ListAsync_WhenVehicleManagementResponds_ShouldReturnVehicles(string route, string status)
    {
        // Arrange
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        HttpStub.Setup(HttpMethod.Get, $"{VehicleManagementUrl}/api/v1/vehicles/{route}", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(new[]
            {
                VehicleManagementApiFactory.Vehicle(first, status),
                VehicleManagementApiFactory.Vehicle(second, status)
            }));

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{route}");
        var body = await ReadResponseAsync<List<VehicleDetail>>(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Data.Select(x => x.Id), Is.EqualTo(new[] { first, second }));
            Assert.That(body.Data.Select(x => x.Status), Is.All.EqualTo(status));
        });
    }

    [TestCase("available")]
    [TestCase("sold")]
    public async Task Vehicles_ListAsync_WhenVehicleManagementFails_ShouldReturn400(string route)
    {
        // Arrange
        HttpStub.Setup(HttpMethod.Get, $"{VehicleManagementUrl}/api/v1/vehicles/{route}", HttpStatusCode.InternalServerError);

        // Act
        var response = await Client.GetAsync($"{Endpoint}/{route}");
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Single().Message, Does.Contain("Failed to get vehicle. Error: Internal Server Error"));
        });
    }
}
