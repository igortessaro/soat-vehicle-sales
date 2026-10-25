using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Moq;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class ListVehicleHandlerTests
{
    private readonly Mock<IVehicleManagementService> _vehicleService = new();

    [SetUp]
    public void SetUp() => _vehicleService.Reset();

    [Test]
    public async Task GetAllAvailable_WhenServiceSucceeds_ReturnsVehicles()
    {
        IReadOnlyList<VehicleDetail> vehicles = [ApplicationTestData.CreateVehicleDetail()];
        _vehicleService.Setup(x => x.GetAllAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicles));

        var response = await new ListVehicleHandler(_vehicleService.Object)
            .Handle(new GetAllAvailableVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(vehicles));
        });
    }

    [Test]
    public async Task GetAllAvailable_WhenServiceFails_ReturnsFailure()
    {
        _vehicleService.Setup(x => x.GetAllAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>("Service unavailable"));

        var response = await new ListVehicleHandler(_vehicleService.Object)
            .Handle(new GetAllAvailableVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Does.Contain("Service unavailable"));
        });
    }

    [Test]
    public async Task GetAllSold_WhenServiceSucceeds_ReturnsVehicles()
    {
        IReadOnlyList<VehicleDetail> vehicles = [ApplicationTestData.CreateVehicleDetail("Sold")];
        _vehicleService.Setup(x => x.GetAllSoldAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicles));

        var response = await new ListVehicleHandler(_vehicleService.Object)
            .Handle(new GetAllSoldVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(vehicles));
        });
    }

    [Test]
    public async Task GetAllSold_WhenServiceFails_ReturnsFailure()
    {
        _vehicleService.Setup(x => x.GetAllSoldAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>("Service unavailable"));

        var response = await new ListVehicleHandler(_vehicleService.Object)
            .Handle(new GetAllSoldVehiclesQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Does.Contain("Service unavailable"));
        });
    }
}
