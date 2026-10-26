using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Integration.Tests.Factories;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Sales;

public sealed class CreateSalesTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/sales";

    [Test]
    public async Task Sales_CreateAsync_WhenVehicleIsAvailable_ShouldReserveVehicleRequestPaymentAndPersistSale()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        string cpf = SaleFactory.CreateValidCpf();
        await SeedPaymentGatewayAsync();
        SetupVehicleFound(vehicleId);
        SetupVehicleReserve(vehicleId, HttpStatusCode.OK);
        HttpStub.Setup(HttpMethod.Post, $"{PaymentGatewayUrl}/api/v1/payments", HttpStatusCode.Created);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = cpf });
        var body = await ReadResponseAsync<SaleDetail>(response);
        var persisted = await FindAsync<Sale>(body.Data.Id);
        var paymentRequest = HttpStub.Requests.Single(x => x.Uri.Host == "payment-gateway");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Data.VehicleId, Is.EqualTo(vehicleId));
            Assert.That(body.Data.BuyerDocument, Is.EqualTo(cpf));
            Assert.That(body.Data.Status, Is.EqualTo(SaleStatus.PendingPayment));
            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted!.PaymentCode, Is.EqualTo(body.Data.PaymentCode));
            Assert.That(paymentRequest.Body, Does.Contain(body.Data.PaymentCode));
            Assert.That(HttpStub.Requests.Select(x => x.Uri.AbsolutePath), Does.Contain($"/api/v1/internal/vehicles/{vehicleId}/reserve"));
        });
    }

    [Test]
    public async Task Sales_CreateAsync_WhenBuyerDocumentIsInvalid_ShouldReturn400WithoutReservingVehicle()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        SetupVehicleFound(vehicleId);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = "12345678900" });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("BuyerDocument 12345678900 is invalid"));
            Assert.That(HttpStub.Requests.Select(x => x.Uri.AbsolutePath), Has.None.EndsWith("/reserve"));
            Assert.That((await ListAsync<Sale>()).Where(x => x.VehicleId == vehicleId), Is.Empty);
        });
    }

    [Test]
    public async Task Sales_CreateAsync_WhenVehicleNotFound_ShouldReturn400()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        HttpStub.Setup(HttpMethod.Get, $"{VehicleManagementUrl}/api/v1/vehicles/{vehicleId}", HttpStatusCode.NotFound);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = SaleFactory.CreateValidCpf() });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Failed to get vehicle. Error: Not Found"));
        });
    }

    [Test]
    public async Task Sales_CreateAsync_WhenVehicleCannotBeReserved_ShouldReturn400WithoutPersistingSale()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        SetupVehicleFound(vehicleId);
        SetupVehicleReserve(vehicleId, HttpStatusCode.BadRequest);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = SaleFactory.CreateValidCpf() });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Failed to reserve vehicle. Error: Bad Request"));
            Assert.That((await ListAsync<Sale>()).Where(x => x.VehicleId == vehicleId), Is.Empty);
        });
    }

    [Test]
    public async Task Sales_CreateAsync_WhenNoPaymentGatewayIsRegistered_ShouldReturn503()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        SetupVehicleFound(vehicleId);
        SetupVehicleReserve(vehicleId, HttpStatusCode.OK);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = SaleFactory.CreateValidCpf() });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("No payment gateway is registered"));
        });
    }

    [Test]
    public async Task Sales_CreateAsync_WhenPaymentGatewayFails_ShouldReturn502()
    {
        // Arrange
        var vehicleId = Guid.NewGuid();
        await SeedPaymentGatewayAsync();
        SetupVehicleFound(vehicleId);
        SetupVehicleReserve(vehicleId, HttpStatusCode.OK);
        HttpStub.Setup(HttpMethod.Post, $"{PaymentGatewayUrl}/api/v1/payments", HttpStatusCode.InternalServerError);

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { VehicleId = vehicleId, BuyerDocument = SaleFactory.CreateValidCpf() });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Payment gateway returned 500 (Internal Server Error)"));
        });
    }

    private void SetupVehicleFound(Guid vehicleId) =>
        HttpStub.Setup(HttpMethod.Get, $"{VehicleManagementUrl}/api/v1/vehicles/{vehicleId}", HttpStatusCode.OK,
            VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(vehicleId)));

    private void SetupVehicleReserve(Guid vehicleId, HttpStatusCode statusCode) =>
        HttpStub.Setup(HttpMethod.Post, $"{VehicleManagementUrl}/api/v1/internal/vehicles/{vehicleId}/reserve", statusCode,
            statusCode == HttpStatusCode.OK ? VehicleManagementApiFactory.Success(VehicleManagementApiFactory.Vehicle(vehicleId, "Reserved")) : null);
}
