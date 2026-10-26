using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Integration.Tests.Factories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Vehicles;

public sealed class SyncVehiclesTests : CustomWebApplicationFactory<Program>
{
    private const string VehiclesUrl = $"{VehicleManagementUrl}/api/v1/vehicles";

    [Test]
    public async Task Vehicles_SyncAsync_ShouldLoadEverythingFirstAndThenOnlyChanges()
    {
        // Arrange
        var available = Guid.NewGuid();
        var sold = Guid.NewGuid();
        HttpStub.Setup(HttpMethod.Get, VehiclesUrl, HttpStatusCode.OK, VehicleManagementApiFactory.Success(new[]
        {
            VehicleManagementApiFactory.Vehicle(available),
            VehicleManagementApiFactory.Vehicle(sold, "Sold")
        }));

        // Act
        var firstSync = await SendAsync(new SyncVehiclesCommand());
        var secondSync = await SendAsync(new SyncVehiclesCommand());
        var persistedAvailable = await FindAsync<Vehicle>(available);
        var persistedSold = await FindAsync<Vehicle>(sold);

        // Assert
        var requests = HttpStub.Requests.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(firstSync.IsSuccess, Is.True);
            Assert.That(secondSync.IsSuccess, Is.True);
            Assert.That(persistedAvailable!.Status, Is.EqualTo(VehicleStatus.Available));
            Assert.That(persistedSold!.Status, Is.EqualTo(VehicleStatus.Sold));
            Assert.That(requests[0].Uri.Query, Is.Empty);
            Assert.That(requests[1].Uri.Query, Does.StartWith("?updatedSince="));
        });
    }

    [Test]
    public async Task Vehicles_SyncAsync_WhenVehicleManagementFails_ShouldReturnFailure()
    {
        // Arrange
        HttpStub.Setup(HttpMethod.Get, VehiclesUrl, HttpStatusCode.ServiceUnavailable);

        // Act
        var response = await SendAsync(new SyncVehiclesCommand());

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Failed to get vehicle. Error: Service Unavailable"));
        });
    }

    private async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(request);
    }
}
