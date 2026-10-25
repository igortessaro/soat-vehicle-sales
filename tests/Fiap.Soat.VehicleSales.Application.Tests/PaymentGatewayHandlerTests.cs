using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Get;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Register;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Update;
using Moq;
using System.Net;
using PaymentGatewayEntity = Fiap.Soat.VehicleSales.Domain.Entities.PaymentGateway;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class PaymentGatewayHandlerTests
{
    private const string InvalidUrlError = "BaseUrl must be an absolute HTTP or HTTPS URL without credentials";
    private readonly Mock<IPaymentGatewayRepository> _repository = new();

    [SetUp]
    public void SetUp() => _repository.Reset();

    [Test]
    public async Task Get_WhenGatewayIsRegistered_ReturnsGateway()
    {
        var gateway = ApplicationTestData.CreatePaymentGateway();
        _repository.Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>())).ReturnsAsync(gateway);

        var response = await new GetPaymentGatewayHandler(_repository.Object)
            .Handle(new GetPaymentGatewayQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(gateway));
        });
    }

    [Test]
    public async Task Get_WhenNoGatewayIsRegistered_ReturnsNotFound()
    {
        _repository.Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>())).ReturnsAsync((PaymentGatewayEntity?) null);

        var response = await new GetPaymentGatewayHandler(_repository.Object)
            .Handle(new GetPaymentGatewayQuery(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("No payment gateway is registered"));
        });
    }

    [TestCase("", "https://gateway.com", "Name is required")]
    [TestCase("   ", "https://gateway.com", "Name is required")]
    [TestCase("Gateway", "", "BaseUrl is required")]
    [TestCase("Gateway", "   ", "BaseUrl is required")]
    [TestCase("Gateway", "gateway.com", InvalidUrlError)]
    [TestCase("Gateway", "ftp://gateway.com", InvalidUrlError)]
    [TestCase("Gateway", "https://user:pass@gateway.com", InvalidUrlError)]
    public async Task Register_WhenRequestIsInvalid_ReturnsFailure(string name, string baseUrl, string error)
    {
        var response = await new RegisterPaymentGatewayHandler(_repository.Object)
            .Handle(new RegisterPaymentGatewayCommand(name, baseUrl), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Reasons[0].Message, Is.EqualTo(error));
        });
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Register_WhenGatewayIsAlreadyRegistered_ReturnsConflict()
    {
        _repository.Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationTestData.CreatePaymentGateway());

        var response = await new RegisterPaymentGatewayHandler(_repository.Object)
            .Handle(new RegisterPaymentGatewayCommand("Gateway", "https://gateway.com"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("A payment gateway is already registered"));
        });
        _repository.Verify(x => x.AddAsync(It.IsAny<PaymentGatewayEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase("http://gateway.com")]
    [TestCase("https://gateway.com/")]
    public async Task Register_WhenRequestIsValid_CreatesGateway(string baseUrl)
    {
        _repository.Setup(x => x.GetRegisteredAsync(It.IsAny<CancellationToken>())).ReturnsAsync((PaymentGatewayEntity?) null);
        _repository.Setup(x => x.AddAsync(It.IsAny<PaymentGatewayEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentGatewayEntity gateway, CancellationToken _) => gateway);

        var response = await new RegisterPaymentGatewayHandler(_repository.Object)
            .Handle(new RegisterPaymentGatewayCommand("  Gateway  ", baseUrl), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Data.Name, Is.EqualTo("Gateway"));
            Assert.That(response.Data.BaseUrl, Is.EqualTo(baseUrl.TrimEnd('/')));
        });
        _repository.Verify(x => x.AddAsync(It.IsAny<PaymentGatewayEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase("", "https://gateway.com", "Name is required")]
    [TestCase("Gateway", "", "BaseUrl is required")]
    [TestCase("Gateway", "not a url", InvalidUrlError)]
    public async Task Update_WhenRequestIsInvalid_ReturnsFailure(string name, string baseUrl, string error)
    {
        var response = await new UpdatePaymentGatewayHandler(_repository.Object)
            .Handle(new UpdatePaymentGatewayCommand(Guid.NewGuid(), name, baseUrl), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo(error));
        });
        _repository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Update_WhenGatewayDoesNotExist_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((PaymentGatewayEntity?) null);

        var response = await new UpdatePaymentGatewayHandler(_repository.Object)
            .Handle(new UpdatePaymentGatewayCommand(id, "Gateway", "https://gateway.com"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Payment gateway not found"));
        });
        _repository.Verify(x => x.UpdateAsync(It.IsAny<PaymentGatewayEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_WhenRequestIsValid_UpdatesGateway()
    {
        var id = Guid.NewGuid();
        var gateway = ApplicationTestData.CreatePaymentGateway();
        _repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(gateway);
        _repository.Setup(x => x.UpdateAsync(gateway, It.IsAny<CancellationToken>())).ReturnsAsync(gateway);

        var response = await new UpdatePaymentGatewayHandler(_repository.Object)
            .Handle(new UpdatePaymentGatewayCommand(id, " New Gateway ", "https://new-gateway.com/"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.SameAs(gateway));
            Assert.That(gateway.Name, Is.EqualTo("New Gateway"));
            Assert.That(gateway.BaseUrl, Is.EqualTo("https://new-gateway.com"));
        });
        _repository.Verify(x => x.UpdateAsync(gateway, It.IsAny<CancellationToken>()), Times.Once);
    }
}
