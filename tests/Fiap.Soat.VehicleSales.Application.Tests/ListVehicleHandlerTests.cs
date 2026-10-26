using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Moq;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class ListVehicleHandlerTests
{
    private static readonly DateTime SourceCreatedAt = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SourceUpdatedAt = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IVehicleRepository> _repository = new();

    [SetUp]
    public void SetUp() => _repository.Reset();

    [Test]
    public async Task GetAllAvailable_ReadsAvailableVehiclesFromTheLocalCopy()
    {
        var vehicle = CreateVehicle(VehicleStatus.Available);
        _repository
            .Setup(x => x.ListByStatusAsync(It.Is<IReadOnlyCollection<VehicleStatus>>(s => s.SequenceEqual(new[] { VehicleStatus.Available })), It.IsAny<CancellationToken>()))
            .ReturnsAsync([vehicle]);

        var response = await new ListVehicleHandler(_repository.Object).Handle(new GetAllAvailableVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data.Single().Id, Is.EqualTo(vehicle.Id));
            Assert.That(response.Data.Single().Status, Is.EqualTo("Available"));
        });
    }

    [Test]
    public async Task GetAllSold_ReadsSoldAndDeliveredVehiclesFromTheLocalCopy()
    {
        var sold = CreateVehicle(VehicleStatus.Sold);
        var delivered = CreateVehicle(VehicleStatus.Delivered);
        _repository
            .Setup(x => x.ListByStatusAsync(It.Is<IReadOnlyCollection<VehicleStatus>>(s => s.SequenceEqual(new[] { VehicleStatus.Sold, VehicleStatus.Delivered })), It.IsAny<CancellationToken>()))
            .ReturnsAsync([sold, delivered]);

        var response = await new ListVehicleHandler(_repository.Object).Handle(new GetAllSoldVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data.Select(x => x.Id), Is.EqualTo(new[] { sold.Id, delivered.Id }));
            Assert.That(response.Data.Select(x => x.Status), Is.EqualTo(new[] { "Sold", "Delivered" }));
        });
    }

    [Test]
    public async Task GetAllAvailable_MapsTheSourceDataAndTimestamps()
    {
        var vehicle = CreateVehicle(VehicleStatus.Available);
        _repository
            .Setup(x => x.ListByStatusAsync(It.IsAny<IReadOnlyCollection<VehicleStatus>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([vehicle]);

        var response = await new ListVehicleHandler(_repository.Object).Handle(new GetAllAvailableVehiclesQuery(), CancellationToken.None);

        var detail = response.Data.Single();
        Assert.Multiple(() =>
        {
            Assert.That(detail.Brand, Is.EqualTo("Toyota"));
            Assert.That(detail.Model, Is.EqualTo("Corolla"));
            Assert.That(detail.LicensePlate, Is.EqualTo("ABC1D23"));
            Assert.That(detail.Year, Is.EqualTo(2024));
            Assert.That(detail.Color, Is.EqualTo("Black"));
            Assert.That(detail.Price, Is.EqualTo(120000m));
            Assert.That(detail.CreatedAt, Is.EqualTo(SourceCreatedAt));
            Assert.That(detail.UpdatedAt, Is.EqualTo(SourceUpdatedAt));
        });
    }

    [Test]
    public async Task GetAllAvailable_WhenTheCopyIsEmpty_ReturnsAnEmptyList()
    {
        _repository
            .Setup(x => x.ListByStatusAsync(It.IsAny<IReadOnlyCollection<VehicleStatus>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await new ListVehicleHandler(_repository.Object).Handle(new GetAllAvailableVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.Empty);
        });
    }

    private static Vehicle CreateVehicle(VehicleStatus status) =>
        new(new VehicleSnapshot(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", 120000m, status, SourceCreatedAt, SourceUpdatedAt));
}
