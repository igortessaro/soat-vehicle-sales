using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class VehicleManagementServiceTests
{
    private const string BaseUrl = "http://vehicle-management";
    private static readonly Guid VehicleId = Guid.Parse("8a1f6f0e-6a55-4c3f-9f0a-1b2c3d4e5f60");

    private static readonly string VehicleJson =
        $$"""{"id":"{{VehicleId}}","brand":"Toyota","model":"Corolla","licensePlate":"ABC1D23","year":2024,"color":"Black","price":100000,"status":"Reserved","createdAt":"2024-01-01T00:00:00Z","updatedAt":"2024-01-01T00:00:00Z"}""";

    private static IEnumerable<TestCaseData> Operations()
    {
        yield return new TestCaseData("reserve", HttpMethod.Post, $"/api/v1/internal/vehicles/{VehicleId}/reserve").SetArgDisplayNames("Reserve");
        yield return new TestCaseData("release", HttpMethod.Post, $"/api/v1/internal/vehicles/{VehicleId}/release").SetArgDisplayNames("Release");
        yield return new TestCaseData("sell", HttpMethod.Post, $"/api/v1/internal/vehicles/{VehicleId}/sell").SetArgDisplayNames("Sell");
        yield return new TestCaseData("deliver", HttpMethod.Post, $"/api/v1/internal/vehicles/{VehicleId}/deliver").SetArgDisplayNames("Deliver");
        yield return new TestCaseData("get", HttpMethod.Get, $"/api/v1/vehicles/{VehicleId}").SetArgDisplayNames("Get");
    }

    [TestCaseSource(nameof(Operations))]
    public async Task Operation_WhenApiSucceeds_CallsEndpointAndReturnsVehicle(string operation, HttpMethod method, string path)
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK, $$"""{"data":{{VehicleJson}},"reasons":[],"isSuccess":true}""");

        var response = await Execute(CreateService(handler), operation);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data.Id, Is.EqualTo(VehicleId));
            Assert.That(response.Data.Brand, Is.EqualTo("Toyota"));
            Assert.That(handler.Requests.Single().Method, Is.EqualTo(method));
            Assert.That(handler.Requests.Single().RequestUri, Is.EqualTo(new Uri(BaseUrl + path)));
        });
    }

    [TestCaseSource(nameof(Operations))]
    public async Task Operation_WhenApiReturnsErrorStatus_ReturnsFailure(string operation, HttpMethod method, string path)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.Conflict);

        var response = await Execute(CreateService(handler), operation);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo($"Failed to {operation} vehicle. Error: Conflict"));
        });
    }

    [TestCaseSource(nameof(Operations))]
    public async Task Operation_WhenApiReturnsNullBody_ReturnsNotFound(string operation, HttpMethod method, string path)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, "null");

        var response = await Execute(CreateService(handler), operation);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo($"Vehicle {VehicleId} not found"));
        });
    }

    [TestCaseSource(nameof(Operations))]
    public async Task Operation_WhenApiReturnsUnsuccessfulBody_ReturnsReasons(string operation, HttpMethod method, string path)
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK, """{"data":null,"reasons":["First","Second"],"isSuccess":false}""");

        var response = await Execute(CreateService(handler), operation);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("First, Second"));
        });
    }

    [TestCaseSource(nameof(Operations))]
    public void Operation_WhenBaseUrlIsNotConfigured_Throws(string operation, HttpMethod method, string path)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK);
        var service = CreateService(handler, baseUrl: null);

        Assert.That(
            async () => await Execute(service, operation),
            Throws.InvalidOperationException.With.Message.EqualTo("VehicleManagement:BaseUrl not configured"));
    }

    [TestCase("available")]
    [TestCase("sold")]
    public async Task GetAll_WhenApiSucceeds_CallsEndpointAndReturnsVehicles(string status)
    {
        var handler = StubHttpMessageHandler.Returning(
            HttpStatusCode.OK, $$"""{"data":[{{VehicleJson}}],"reasons":[],"isSuccess":true}""");

        var response = await ExecuteGetAll(CreateService(handler), status);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data.Single().Id, Is.EqualTo(VehicleId));
            Assert.That(handler.Requests.Single().Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(handler.Requests.Single().RequestUri, Is.EqualTo(new Uri($"{BaseUrl}/api/v1/vehicles/{status}")));
        });
    }

    [TestCase("available")]
    [TestCase("sold")]
    public async Task GetAll_WhenApiReturnsErrorStatus_ReturnsFailure(string status)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.InternalServerError);

        var response = await ExecuteGetAll(CreateService(handler), status);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Reasons[0].Message, Is.EqualTo("Failed to get vehicle. Error: Internal Server Error"));
        });
    }

    [TestCase("available")]
    [TestCase("sold")]
    public async Task GetAll_WhenApiReturnsNullBody_ReturnsEmptyList(string status)
    {
        var handler = StubHttpMessageHandler.Returning(HttpStatusCode.OK, "null");

        var response = await ExecuteGetAll(CreateService(handler), status);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.Data, Is.Empty);
        });
    }

    [TestCase("available")]
    [TestCase("sold")]
    public void GetAll_WhenBaseUrlIsNotConfigured_Throws(string status)
    {
        var service = CreateService(StubHttpMessageHandler.Returning(HttpStatusCode.OK), baseUrl: null);

        Assert.That(async () => await ExecuteGetAll(service, status), Throws.InvalidOperationException);
    }

    private static Task<Response<VehicleDetail>> Execute(VehicleManagementService service, string operation) =>
        operation switch
        {
            "reserve" => service.ReserveVehicleAsync(VehicleId),
            "release" => service.ReleaseVehicleAsync(VehicleId),
            "sell" => service.SellVehicleAsync(VehicleId),
            "deliver" => service.DeliverVehicleAsync(VehicleId),
            _ => service.GetAsync(VehicleId)
        };

    private static Task<Response<IReadOnlyList<VehicleDetail>>> ExecuteGetAll(VehicleManagementService service, string status) =>
        status == "available" ? service.GetAllAvailableAsync() : service.GetAllSoldAsync();

    private static VehicleManagementService CreateService(StubHttpMessageHandler handler, string? baseUrl = BaseUrl)
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x["VehicleManagement:BaseUrl"]).Returns(baseUrl);

        return new VehicleManagementService(
            NullLogger<VehicleManagementService>.Instance,
            InfrastructureTestFixture.CreateHttpClientFactory(handler).Object,
            configuration.Object);
    }
}
