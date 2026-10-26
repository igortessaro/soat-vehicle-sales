using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Integration.Tests.Factories;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Webhooks;

public sealed class PaymentWebhookTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/webhooks/payments";

    [Test]
    public async Task Webhooks_ProcessAsync_WhenPaymentIsPaid_ShouldSellVehicleAndMarkSaleAsPaid()
    {
        // Arrange
        var sale = await SeedAsync(SaleFactory.CreateSale());
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/sell", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(sale.VehicleId, "Sold")));

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { sale.PaymentCode, Status = "Paid" });
        var persisted = await FindAsync<Sale>(sale.Id);
        var vehicleCopy = await FindAsync<Vehicle>(sale.VehicleId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(persisted.PaidAt, Is.Not.Null);
            Assert.That(vehicleCopy!.Status, Is.EqualTo(VehicleStatus.Sold));
        });
    }

    [Test]
    public async Task Webhooks_ProcessAsync_WhenTheLocalCopyCannotBeUpdated_ShouldStillMarkSaleAsPaid()
    {
        // Arrange: an unknown status makes the copy update fail after the sale was already saved
        var sale = await SeedAsync(SaleFactory.CreateSale());
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/sell", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(sale.VehicleId, "Scrapped")));

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { sale.PaymentCode, Status = "Paid" });
        var persisted = await FindAsync<Sale>(sale.Id);
        var vehicleCopy = await FindAsync<Vehicle>(sale.VehicleId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(vehicleCopy, Is.Null);
        });
    }

    [Test]
    public async Task Webhooks_ProcessAsync_WhenPaymentIsCanceled_ShouldReleaseVehicleAndCancelSale()
    {
        // Arrange
        var sale = await SeedAsync(SaleFactory.CreateSale());
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/release", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(sale.VehicleId)));

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { sale.PaymentCode, Status = "Canceled" });
        var persisted = await FindAsync<Sale>(sale.Id);
        var vehicleCopy = await FindAsync<Vehicle>(sale.VehicleId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.Canceled));
            Assert.That(persisted.CanceledAt, Is.Not.Null);
            Assert.That(vehicleCopy!.Status, Is.EqualTo(VehicleStatus.Available));
        });
    }

    [TestCase("Paid")]
    [TestCase("Canceled")]
    public async Task Webhooks_ProcessAsync_WhenPaymentCodeIsUnknown_ShouldReturn404(string status)
    {
        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { PaymentCode = "unknown-code", Status = status });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Sale with payment code unknown-code not found"));
            Assert.That(HttpStub.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task Webhooks_ProcessAsync_WhenVehicleCannotBeSold_ShouldReturn400AndKeepSalePending()
    {
        // Arrange
        var sale = await SeedAsync(SaleFactory.CreateSale());
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{sale.VehicleId}/sell", HttpStatusCode.Conflict);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { sale.PaymentCode, Status = "Paid" });
        var error = await ReadErrorAsync(response);
        var persisted = await FindAsync<Sale>(sale.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Failed to sell vehicle. Error: Conflict"));
            Assert.That(persisted!.Status, Is.EqualTo(SaleStatus.PendingPayment));
        });
    }
}
