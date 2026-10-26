using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Copy;
using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using MediatR;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class PaymentHandlerTests
{
    private const string PaymentCode = "payment-code";
    private readonly Mock<ISaleRepository> _repository = new();
    private readonly Mock<IVehicleManagementService> _vehicleService = new();
    private readonly Mock<IPublisher> _publisher = new();

    [SetUp]
    public void SetUp()
    {
        _repository.Reset();
        _vehicleService.Reset();
        _publisher.Reset();
    }

    [Test]
    public async Task Paid_WhenSaleDoesNotExist_ReturnsFailure()
    {
        _repository.Setup(x => x.GetByPaymentCodeAsync(PaymentCode, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?) null);

        var response = await CreateHandler().Handle(new PaymentPaidCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Reasons[0].Message, Is.EqualTo($"Sale with payment code {PaymentCode} not found"));
        });
        _vehicleService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Paid_WhenVehicleCannotBeSold_ReturnsFailure()
    {
        var sale = SetupSale();
        _vehicleService
            .Setup(x => x.SellVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<VehicleDetail>("Vehicle is not reserved", HttpStatusCode.Conflict));

        var response = await CreateHandler().Handle(new PaymentPaidCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Vehicle is not reserved"));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.PendingPayment));
        });
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<VehicleChangedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Paid_WhenVehicleIsSold_MarksSaleAsPaid()
    {
        var sale = SetupSale();
        var vehicle = ApplicationTestData.CreateVehicleDetail("Sold");
        _vehicleService
            .Setup(x => x.SellVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicle));

        var response = await CreateHandler().Handle(new PaymentPaidCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(sale.PaidAt, Is.Not.Null);
        });
        _repository.Verify(x => x.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(x => x.Publish(new VehicleChangedNotification(vehicle), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Canceled_WhenSaleDoesNotExist_ReturnsFailure()
    {
        _repository.Setup(x => x.GetByPaymentCodeAsync(PaymentCode, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?) null);

        var response = await CreateHandler().Handle(new PaymentCanceledCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Reasons[0].Message, Is.EqualTo($"Sale with payment code {PaymentCode} not found"));
        });
        _vehicleService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Canceled_WhenVehicleCannotBeReleased_ReturnsFailure()
    {
        var sale = SetupSale();
        _vehicleService
            .Setup(x => x.ReleaseVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<VehicleDetail>("Vehicle cannot be released", HttpStatusCode.Conflict));

        var response = await CreateHandler().Handle(new PaymentCanceledCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Vehicle cannot be released"));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.PendingPayment));
        });
        _repository.Verify(x => x.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.Publish(It.IsAny<VehicleChangedNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Canceled_WhenVehicleIsReleased_MarksSaleAsCanceled()
    {
        var sale = SetupSale();
        var vehicle = ApplicationTestData.CreateVehicleDetail();
        _vehicleService
            .Setup(x => x.ReleaseVehicleAsync(sale.VehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicle));

        var response = await CreateHandler().Handle(new PaymentCanceledCommand(PaymentCode), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Canceled));
            Assert.That(sale.CanceledAt, Is.Not.Null);
        });
        _repository.Verify(x => x.UpdateAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(x => x.Publish(new VehicleChangedNotification(vehicle), It.IsAny<CancellationToken>()), Times.Once);
    }

    private PaymentHandler CreateHandler() => new(_repository.Object, _vehicleService.Object, _publisher.Object);

    private Sale SetupSale()
    {
        var sale = ApplicationTestData.CreateSale();
        _repository.Setup(x => x.GetByPaymentCodeAsync(PaymentCode, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _repository.Setup(x => x.UpdateAsync(sale, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        return sale;
    }
}
