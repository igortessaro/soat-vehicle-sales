using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Integration.Tests.Factories;
using System.Net;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Sales;

public sealed class DeliverSalesTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/sales";

    [Test]
    public async Task Sales_DeliverAsync_WhenVehicleIsDelivered_ShouldCompleteSale()
    {
        // Arrange
        var sale = await SeedAsync(SaleFactory.CreateSale(SaleStatus.Paid));
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/deliver", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(sale.VehicleId, "Delivered")));

        // Act
        var response = await Client.PutAsync($"{Endpoint}/{sale.Id}/deliver", null);
        var body = await ReadResponseAsync<SaleDetail>(response);
        var persisted = await FindAsync<Sale>(sale.Id);
        var vehicleCopy = await FindAsync<Vehicle>(sale.VehicleId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(vehicleCopy!.Status, Is.EqualTo(VehicleStatus.Delivered));
            Assert.That(body.Data.Status, Is.EqualTo(SaleStatus.Completed));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.Completed));
            Assert.That(body.Data.UpdatedAt, Is.GreaterThan(body.Data.CreatedAt));
            Assert.That(persisted.UpdatedAt, Is.EqualTo(body.Data.UpdatedAt));
        });
    }

    [Test]
    public async Task Sales_DeliverAsync_WhenSaleNotFound_ShouldReturn400()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.PutAsync($"{Endpoint}/{id}/deliver", null);
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member($"Sale with id {id} not found"));
            Assert.That(HttpStub.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task Sales_DeliverAsync_WhenVehicleCannotBeDelivered_ShouldReturn400AndKeepSaleStatus()
    {
        // Arrange
        var sale = await SeedAsync(SaleFactory.CreateSale(SaleStatus.Paid));
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/deliver", HttpStatusCode.BadRequest);

        // Act
        var response = await Client.PutAsync($"{Endpoint}/{sale.Id}/deliver", null);
        var error = await ReadErrorAsync(response);
        var persisted = await FindAsync<Sale>(sale.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Failed to deliver vehicle. Error: Bad Request"));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.Paid));
        });
    }
}
