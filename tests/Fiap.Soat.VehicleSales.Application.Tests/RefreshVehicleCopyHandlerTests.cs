using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Copy;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class RefreshVehicleCopyHandlerTests
{
    private readonly Mock<IVehicleRepository> _repository = new();
    private readonly Mock<ILogger<RefreshVehicleCopyHandler>> _logger = new();

    [SetUp]
    public void SetUp()
    {
        _repository.Reset();
        _logger.Reset();
    }

    [Test]
    public async Task Handle_UpsertsTheVehicleSnapshot()
    {
        var vehicle = ApplicationTestData.CreateVehicleDetail("Sold");
        _repository
            .Setup(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleSnapshot snapshot, CancellationToken _) => new Vehicle(snapshot));

        await CreateHandler().Handle(new VehicleChangedNotification(vehicle), CancellationToken.None);

        _repository.Verify(x => x.UpsertAsync(
            It.Is<VehicleSnapshot>(s => s.Id == vehicle.Id && s.Status == VehicleStatus.Sold && s.UpdatedAt == vehicle.UpdatedAt),
            It.IsAny<CancellationToken>()), Times.Once);
        VerifyWarningLogged(Times.Never());
    }

    [Test]
    public async Task Handle_WhenTheCopyCannotBeSaved_LogsAWarningInsteadOfThrowing()
    {
        _repository
            .Setup(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Duplicate entry"));

        await CreateHandler().Handle(new VehicleChangedNotification(ApplicationTestData.CreateVehicleDetail()), CancellationToken.None);

        VerifyWarningLogged(Times.Once());
    }

    [Test]
    public async Task Handle_WhenTheStatusIsUnknown_LogsAWarningInsteadOfThrowing()
    {
        await CreateHandler().Handle(new VehicleChangedNotification(ApplicationTestData.CreateVehicleDetail("Scrapped")), CancellationToken.None);

        _repository.Verify(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyWarningLogged(Times.Once());
    }

    [Test]
    public void Handle_WhenTheRequestIsCanceled_PropagatesTheCancellation()
    {
        _repository
            .Setup(x => x.UpsertAsync(It.IsAny<VehicleSnapshot>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        Assert.That(
            async () => await CreateHandler().Handle(new VehicleChangedNotification(ApplicationTestData.CreateVehicleDetail()), CancellationToken.None),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private RefreshVehicleCopyHandler CreateHandler() => new(_repository.Object, _logger.Object);

    private void VerifyWarningLogged(Times times) =>
        _logger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
}
