using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class DeliverSaleHandlerTests
{
    private readonly Mock<ISaleRepository> _repository = new();
    private readonly Mock<IVehicleManagementService> _vehicleService = new();

    [SetUp]
    public void SetUp()
    {
        _repository.Reset();
        _vehicleService.Reset();
    }

    [Test]
    public async Task Handle_WhenSaleDoesNotExist_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?) null);

        var response = await CreateHandler().Handle(new DeliverSaleCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Reasons[0].Message, Is.EqualTo($"Sale with id {id} not found"));
        });
        _vehicleService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_WhenVehicleCannotBeDelivered_ReturnsFailure()
    {
        var id = Guid.NewGuid();
        var sale = ApplicationTestData.CreateSale(SaleStatus.Paid);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _vehicleService
            .Setup(x => x.DeliverVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<VehicleDetail>("Vehicle is not sold"));

        var response = await CreateHandler().Handle(new DeliverSaleCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Vehicle is not sold"));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Paid));
        });
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenVehicleIsDelivered_CompletesSale()
    {
        var id = Guid.NewGuid();
        var sale = ApplicationTestData.CreateSale(SaleStatus.Paid);
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _repository.Setup(x => x.UpdateAsync(sale, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _vehicleService
            .Setup(x => x.DeliverVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(ApplicationTestData.CreateVehicleDetail("Delivered")));

        var response = await CreateHandler().Handle(new DeliverSaleCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(sale));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Completed));
        });
        _repository.Verify(x => x.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
    }

    private DeliverSaleHandler CreateHandler() => new(_repository.Object, _vehicleService.Object);
}
