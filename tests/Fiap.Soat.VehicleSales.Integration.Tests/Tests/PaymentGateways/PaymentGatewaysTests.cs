using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.PaymentGateways;

public sealed class PaymentGatewaysTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/payment-gateways";

    // Ordered because the gateway is a singleton: the first test runs against an empty database and the rest seed it.
    [Test, Order(1)]
    public async Task PaymentGateways_GetAsync_WhenNoGatewayIsRegistered_ShouldReturn404()
    {
        // Act
        var response = await Client.GetAsync(Endpoint);
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("No payment gateway is registered"));
        });
    }

    [TestCase("", "https://gateway.com", "Name is required"), Order(2)]
    [TestCase("Gateway", "", "BaseUrl is required")]
    [TestCase("Gateway", "ftp://gateway.com", "BaseUrl must be an absolute HTTP or HTTPS URL without credentials")]
    public async Task PaymentGateways_RegisterAsync_WhenRequestIsInvalid_ShouldReturn400(string name, string baseUrl, string expectedMessage)
    {
        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { Name = name, BaseUrl = baseUrl });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member(expectedMessage));
        });
    }

    [Test, Order(3)]
    public async Task PaymentGateways_GetAsync_WhenGatewayIsRegistered_ShouldReturn200()
    {
        // Arrange
        var gateway = await SeedPaymentGatewayAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);
        var body = await ReadResponseAsync<PaymentGatewayDetail>(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Data.Id, Is.EqualTo(gateway.Id));
            Assert.That(body.Data.BaseUrl, Is.EqualTo(PaymentGatewayUrl));
        });
    }

    [Test, Order(4)]
    public async Task PaymentGateways_RegisterAsync_WhenGatewayAlreadyRegistered_ShouldReturn409()
    {
        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { Name = "Another", BaseUrl = "https://another.com" });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("A payment gateway is already registered"));
        });
    }

    [Test, Order(5)]
    public async Task PaymentGateways_UpdateAsync_WhenRequestIsValid_ShouldPersistChanges()
    {
        // Arrange
        var gateway = (await ListAsync<PaymentGateway>()).Single();

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{gateway.Id}", new { Name = " Updated Gateway ", BaseUrl = "https://updated.com/" });
        var persisted = await FindAsync<PaymentGateway>(gateway.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(persisted!.Name, Is.EqualTo("Updated Gateway"));
            Assert.That(persisted.BaseUrl, Is.EqualTo("https://updated.com"));
        });
    }

    [Test, Order(6)]
    public async Task PaymentGateways_UpdateAsync_WhenGatewayNotFound_ShouldReturn404()
    {
        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/{Guid.NewGuid()}", new { Name = "Gateway", BaseUrl = "https://gateway.com" });
        var error = await ReadErrorAsync(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(error.Errors.Select(x => x.Message), Has.Member("Payment gateway not found"));
        });
    }
}
