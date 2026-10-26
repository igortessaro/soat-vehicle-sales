using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Sync;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class SyncVehiclesHandlerTests
{
    private readonly Mock<IVehicleRepository> _repository = new();
    private readonly Mock<IVehicleManagementService> _vehicleService = new();

    [SetUp]
    public void SetUp()
    {
        _repository.Reset();
        _vehicleService.Reset();
        _repository
            .Setup(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleSnapshot snapshot, CancellationToken _) => new Vehicle(snapshot));
    }

    [Test]
    public async Task Handle_WhenLocalCopyIsEmpty_LoadsEveryVehicle()
    {
        IReadOnlyList<VehicleDetail> vehicles = [ApplicationTestData.CreateVehicleDetail(), ApplicationTestData.CreateVehicleDetail("Sold")];
        _repository.Setup(x => x.GetLatestSourceUpdatedAtAsync(It.IsAny<CancellationToken>())).ReturnsAsync((DateTime?) null);
        _vehicleService.Setup(x => x.GetUpdatedSinceAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(ResponseFactory.Ok(vehicles));

        var response = await CreateHandler().Handle(new SyncVehiclesCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.EqualTo(2));
        });
        _repository.Verify(x => x.UpsertAsync(It.Is<VehicleSnapshot>(s => s.Id == vehicles[0].Id), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(x => x.UpsertAsync(It.Is<VehicleSnapshot>(s => s.Id == vehicles[1].Id && s.Status == VehicleStatus.Sold), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenLocalCopyHasVehicles_RequestsOnlyChangesSinceTheLatestUpdate()
    {
        var latest = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        using var cancellation = new CancellationTokenSource();
        _repository.Setup(x => x.GetLatestSourceUpdatedAtAsync(cancellation.Token)).ReturnsAsync(latest);
        _vehicleService.Setup(x => x.GetUpdatedSinceAsync(latest, cancellation.Token))
            .ReturnsAsync(ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>([]));

        var response = await CreateHandler().Handle(new SyncVehiclesCommand(), cancellation.Token);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.Zero);
        });
        _vehicleService.Verify(x => x.GetUpdatedSinceAsync(latest, cancellation.Token), Times.Once);
        _repository.Verify(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenVehicleManagementFails_ReturnsFailureWithoutTouchingTheCopy()
    {
        _repository.Setup(x => x.GetLatestSourceUpdatedAtAsync(It.IsAny<CancellationToken>())).ReturnsAsync((DateTime?) null);
        _vehicleService.Setup(x => x.GetUpdatedSinceAsync(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>("Failed to get vehicle. Error: Service Unavailable", HttpStatusCode.ServiceUnavailable));

        var response = await CreateHandler().Handle(new SyncVehiclesCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Failed to get vehicle. Error: Service Unavailable"));
        });
        _repository.Verify(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private SyncVehiclesHandler CreateHandler() => new(_repository.Object, _vehicleService.Object);
}
