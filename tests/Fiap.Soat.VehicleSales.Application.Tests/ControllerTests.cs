using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Get;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Register;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Update;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.List;
using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using Moq;
using System.Net;
using PaymentGatewayEntity = Fiap.Soat.VehicleSales.Domain.Entities.PaymentGateway;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class ControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [SetUp]
    public void SetUp() => _mediator.Reset();

    [Test]
    public async Task SaleController_CreateAsync_SendsOriginalCommandAndMapsResponse()
    {
        var sale = ApplicationTestData.CreateSale();
        var command = new CreateSaleCommand(sale.VehicleId, ApplicationTestData.ValidDocument);
        _mediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(sale, HttpStatusCode.Created));

        var response = await new SaleController(_mediator.Object).CreateAsync(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data.PaymentCode, Is.EqualTo(sale.PaymentCode));
        });
        _mediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SaleController_DeliverAsync_SendsCommandWithSaleId()
    {
        var id = Guid.NewGuid();
        _mediator.Setup(x => x.Send(It.IsAny<DeliverSaleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<Sale>("Sale not found", HttpStatusCode.NotFound));

        var response = await new SaleController(_mediator.Object).DeliverAsync(id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
        _mediator.Verify(x => x.Send(It.Is<DeliverSaleCommand>(c => c.SaleId == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task VehicleController_GetAllAvailableVehicles_SendsQuery()
    {
        IReadOnlyList<VehicleDetail> vehicles = [ApplicationTestData.CreateVehicleDetail()];
        _mediator.Setup(x => x.Send(It.IsAny<GetAllAvailableVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicles));

        var response = await new VehicleController(_mediator.Object).GetAllAvailableVehicles(CancellationToken.None);

        Assert.That(response.Data, Is.SameAs(vehicles));
        _mediator.Verify(x => x.Send(It.IsAny<GetAllAvailableVehiclesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task VehicleController_GetAllSoldVehicles_SendsQuery()
    {
        IReadOnlyList<VehicleDetail> vehicles = [ApplicationTestData.CreateVehicleDetail("Sold")];
        _mediator.Setup(x => x.Send(It.IsAny<GetAllSoldVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(vehicles));

        var response = await new VehicleController(_mediator.Object).GetAllSoldVehicles(CancellationToken.None);

        Assert.That(response.Data, Is.SameAs(vehicles));
        _mediator.Verify(x => x.Send(It.IsAny<GetAllSoldVehiclesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task WebhookController_WhenPaymentIsPaid_SendsPaidCommand()
    {
        _mediator.Setup(x => x.Send(It.IsAny<PaymentPaidCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok());

        var response = await new WebhookController(_mediator.Object)
            .ProcessAsync(new PaymentCommand("code", PaymentStatus.Paid), CancellationToken.None);

        Assert.That(response.IsSuccess, Is.True);
        _mediator.Verify(x => x.Send(It.Is<PaymentPaidCommand>(c => c.PaymentCode == "code"), It.IsAny<CancellationToken>()), Times.Once);
        _mediator.Verify(x => x.Send(It.IsAny<PaymentCanceledCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task WebhookController_WhenPaymentIsCanceled_SendsCanceledCommand()
    {
        _mediator.Setup(x => x.Send(It.IsAny<PaymentCanceledCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok());

        var response = await new WebhookController(_mediator.Object)
            .ProcessAsync(new PaymentCommand("code", PaymentStatus.Canceled), CancellationToken.None);

        Assert.That(response.IsSuccess, Is.True);
        _mediator.Verify(x => x.Send(It.Is<PaymentCanceledCommand>(c => c.PaymentCode == "code"), It.IsAny<CancellationToken>()), Times.Once);
        _mediator.Verify(x => x.Send(It.IsAny<PaymentPaidCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task WebhookController_WhenPaymentStatusIsUnknown_ReturnsBadRequestWithoutSendingCommand()
    {
        var response = await new WebhookController(_mediator.Object)
            .ProcessAsync(new PaymentCommand("code", (PaymentStatus) 99), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Payment status 99 is not supported"));
        });
        _mediator.VerifyNoOtherCalls();
    }

    [Test]
    public async Task PaymentGatewayController_RegisterAsync_SendsCommandAndMapsResponse()
    {
        var gateway = ApplicationTestData.CreatePaymentGateway();
        _mediator.Setup(x => x.Send(It.IsAny<RegisterPaymentGatewayCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(gateway, HttpStatusCode.Created));

        var response = await new PaymentGatewayController(_mediator.Object)
            .RegisterAsync("Gateway", "https://gateway.com", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data.Name, Is.EqualTo(gateway.Name));
            Assert.That(response.Data.BaseUrl, Is.EqualTo(gateway.BaseUrl));
        });
        _mediator.Verify(x => x.Send(
            It.Is<RegisterPaymentGatewayCommand>(c => c.Name == "Gateway" && c.BaseUrl == "https://gateway.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PaymentGatewayController_GetAsync_SendsQuery()
    {
        _mediator.Setup(x => x.Send(It.IsAny<GetPaymentGatewayQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Ok(ApplicationTestData.CreatePaymentGateway()));

        var response = await new PaymentGatewayController(_mediator.Object).GetAsync(CancellationToken.None);

        Assert.That(response.IsSuccess, Is.True);
        _mediator.Verify(x => x.Send(It.IsAny<GetPaymentGatewayQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PaymentGatewayController_UpdateAsync_SendsCommandAndMapsFailure()
    {
        var id = Guid.NewGuid();
        var errors = new List<FluentResults.IError> { new FluentResults.Error("First"), new FluentResults.Error("Second") };
        _mediator.Setup(x => x.Send(It.IsAny<UpdatePaymentGatewayCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.Fail<PaymentGatewayEntity>(errors, HttpStatusCode.UnprocessableEntity));

        var response = await new PaymentGatewayController(_mediator.Object)
            .UpdateAsync(id, "Gateway", "https://gateway.com", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("First, Second"));
        });
        _mediator.Verify(x => x.Send(It.Is<UpdatePaymentGatewayCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }
}
