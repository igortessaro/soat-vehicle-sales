using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

internal static class InfrastructureTestFixture
{
    public const string ValidDocument = "52998224725";

    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static Mock<IHttpClientFactory> CreateHttpClientFactory(StubHttpMessageHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler, disposeHandler: false));
        return factory;
    }
}

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];
    public List<string?> RequestBodies { get; } = [];

    public static StubHttpMessageHandler Returning(HttpStatusCode statusCode, string? json = null) =>
        new(_ => new HttpResponseMessage(statusCode)
        {
            Content = json is null ? null : new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });

    public static StubHttpMessageHandler Throwing(Exception exception) =>
        new(_ => throw exception);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestBodies.Add(request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken));
        return responder(request);
    }
}
