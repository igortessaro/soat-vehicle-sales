using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using System.Net;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Vehicles;

public sealed class ListVehiclesTests : CustomWebApplicationFactory<Program>
{
    private const string Endpoint = "/api/v1/vehicles";

    private Vehicle _available = null!;
    private Vehicle _cheaperAvailable = null!;
    private Vehicle _sold = null!;
    private Vehicle _delivered = null!;

    [OneTimeSetUp]
    public async Task SeedLocalCopy()
    {
        _available = await SeedAsync(CreateVehicle(VehicleStatus.Available, 120000m));
        _cheaperAvailable = await SeedAsync(CreateVehicle(VehicleStatus.Available, 80000m));
        await SeedAsync(CreateVehicle(VehicleStatus.Reserved, 90000m));
        _sold = await SeedAsync(CreateVehicle(VehicleStatus.Sold, 100000m));
        _delivered = await SeedAsync(CreateVehicle(VehicleStatus.Delivered, 70000m));
    }

    [Test]
    public async Task Vehicles_ListAvailableAsync_ShouldReturnAvailableVehiclesFromTheLocalCopyOrderedByPrice()
    {
        // Act
        var response = await Client.GetAsync($"{Endpoint}/available");
        var body = await ReadResponseAsync<List<VehicleDetail>>(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Data.Select(x => x.Id), Is.EqualTo(new[] { _cheaperAvailable.Id, _available.Id }));
            Assert.That(body.Data.Select(x => x.Status), Is.All.EqualTo("Available"));
            Assert.That(HttpStub.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task Vehicles_ListSoldAsync_ShouldReturnSoldAndDeliveredVehiclesFromTheLocalCopyOrderedByPrice()
    {
        // Act
        var response = await Client.GetAsync($"{Endpoint}/sold");
        var body = await ReadResponseAsync<List<VehicleDetail>>(response);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body.Data.Select(x => x.Id), Is.EqualTo(new[] { _delivered.Id, _sold.Id }));
            Assert.That(body.Data.Select(x => x.Status), Is.EqualTo(new[] { "Delivered", "Sold" }));
            Assert.That(HttpStub.Requests, Is.Empty);
        });
    }

    [Test]
    public async Task Vehicles_ListAvailableAsync_ShouldKeepTheVehicleContract()
    {
        // Act
        var response = await Client.GetAsync($"{Endpoint}/available");
        var body = await ReadResponseAsync<List<VehicleDetail>>(response);

        // Assert
        var detail = body.Data.Single(x => x.Id == _available.Id);
        Assert.Multiple(() =>
        {
            Assert.That(detail.Brand, Is.EqualTo(_available.Brand));
            Assert.That(detail.Model, Is.EqualTo(_available.Model));
            Assert.That(detail.LicensePlate, Is.EqualTo(_available.LicensePlate));
            Assert.That(detail.Year, Is.EqualTo(_available.ManufactureYear));
            Assert.That(detail.Color, Is.EqualTo(_available.Color));
            Assert.That(detail.Price, Is.EqualTo(_available.Price));
            Assert.That(detail.CreatedAt, Is.EqualTo(_available.SourceCreatedAt));
            Assert.That(detail.UpdatedAt, Is.EqualTo(_available.SourceUpdatedAt));
        });
    }

    private Vehicle CreateVehicle(VehicleStatus status, decimal price)
    {
        var updatedAt = DateTime.UtcNow;
        return new Vehicle(new VehicleSnapshot(
            Guid.NewGuid(),
            Faker.Vehicle.Manufacturer(),
            Faker.Vehicle.Model(),
            Faker.Random.Replace("???#?##").ToUpperInvariant(),
            Faker.Random.Int(2015, updatedAt.Year),
            Faker.Commerce.Color(),
            price,
            status,
            updatedAt.AddDays(-1),
            updatedAt));
    }
}
