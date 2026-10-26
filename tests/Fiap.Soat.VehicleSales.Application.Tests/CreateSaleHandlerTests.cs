using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class CreateSaleHandlerTests
{
    private readonly Mock<ISaleRepository> _repository = new();
    private readonly Mock<IVehicleManagementService> _vehicleService = new();
    private readonly Mock<IPaymentGatewayService> _paymentService = new();
    private readonly Guid _vehicleId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _repository.Reset();
        _vehicleService.Reset();
        _paymentService.Reset();
    }

    [Test]
    public async Task Handle_WhenVehicleIdIsEmpty_ReturnsFailure()
    {
        var response = await CreateHandler().Handle(
            new CreateSaleCommand(Guid.Empty, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("VehicleId is required"));
        });
        VerifyNoOtherCalls();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task Handle_WhenBuyerDocumentIsEmpty_ReturnsFailure(string document)
    {
        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, document),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("BuyerDocument is required"));
        });
        VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_WhenVehicleIsNotFound_ReturnsFailure()
    {
        _vehicleService
            .Setup(x => x.GetAsync(_vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<VehicleDetail>("Vehicle not found", HttpStatusCode.NotFound));

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Vehicle not found"));
        });
        _vehicleService.Verify(x => x.ReserveVehicleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_WhenBuyerDocumentIsInvalid_ReturnsFailure()
    {
        SetupVehicleFound();

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, "12345678900"),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("BuyerDocument 12345678900 is invalid"));
        });
        _vehicleService.Verify(x => x.ReserveVehicleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_WhenVehicleCannotBeReserved_ReturnsFailureWithServiceStatusCode()
    {
        SetupVehicleFound();
        _vehicleService
            .Setup(x => x.ReserveVehicleAsync(_vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<VehicleDetail>("Vehicle is not available", HttpStatusCode.Conflict));

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Vehicle is not available"));
        });
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Handle_WhenPaymentCreationFails_ReturnsFailureWithServiceStatusCode()
    {
        SetupVehicleFound();
        SetupVehicleReserved();
        var sale = SetupSaleAdded();
        _paymentService
            .Setup(x => x.CreatePaymentAsync(sale.PaymentCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail("Gateway unavailable", HttpStatusCode.ServiceUnavailable));

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Gateway unavailable"));
        });
        _repository.Verify(x => x.FindSingleAsync(It.IsAny<Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_WhenSaleCannotBeReloaded_ReturnsFailure()
    {
        SetupVehicleFound();
        SetupVehicleReserved();
        var sale = SetupSaleAdded();
        SetupPaymentCreated(sale);
        _repository
            .Setup(x => x.FindSingleAsync(It.IsAny<Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?) null);

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Sale could not be loaded after payment processing"));
        });
    }

    [Test]
    public async Task Handle_WhenRequestIsValid_CreatesSaleAndReturnsReloadedSale()
    {
        SetupVehicleFound();
        SetupVehicleReserved();
        var sale = SetupSaleAdded();
        SetupPaymentCreated(sale);
        var reloaded = ApplicationTestData.CreateSale(vehicleId: _vehicleId);
        _repository
            .Setup(x => x.FindSingleAsync(It.IsAny<Expression<Func<Sale, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);

        var response = await CreateHandler().Handle(
            new CreateSaleCommand(_vehicleId, ApplicationTestData.ValidDocument),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(reloaded));
        });
        _repository.Verify(x => x.AddAsync(
            It.Is<Sale>(s => s.VehicleId == _vehicleId && s.BuyerDocument.Value == ApplicationTestData.ValidDocument),
            It.IsAny<CancellationToken>()), Times.Once);
        _vehicleService.Verify(x => x.ReserveVehicleAsync(_vehicleId, It.IsAny<CancellationToken>()), Times.Once);
        _paymentService.Verify(x => x.CreatePaymentAsync(sale.PaymentCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    private CreateSaleHandler CreateHandler() =>
        new(_repository.Object, _vehicleService.Object, _paymentService.Object);

    private void SetupVehicleFound() =>
        _vehicleService
            .Setup(x => x.GetAsync(_vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(ApplicationTestData.CreateVehicleDetail()));

    private void SetupVehicleReserved() =>
        _vehicleService
            .Setup(x => x.ReserveVehicleAsync(_vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(ApplicationTestData.CreateVehicleDetail("Reserved")));

    private Sale SetupSaleAdded()
    {
        var sale = ApplicationTestData.CreateSale(vehicleId: _vehicleId);
        _repository
            .Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);
        return sale;
    }

    private void SetupPaymentCreated(Sale sale) =>
        _paymentService
            .Setup(x => x.CreatePaymentAsync(sale.PaymentCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok());

    private void VerifyNoOtherCalls()
    {
        _repository.VerifyNoOtherCalls();
        _vehicleService.VerifyNoOtherCalls();
        _paymentService.VerifyNoOtherCalls();
    }
}
