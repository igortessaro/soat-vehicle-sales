using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Fiap.Soat.VehicleSales.Api.Tests;

internal sealed class EndpointTestServer : IAsyncDisposable
{
    private readonly WebApplication _app;

    private EndpointTestServer(WebApplication app)
    {
        _app = app;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public static async Task<EndpointTestServer> StartAsync<TController>(TController controller, Func<WebApplication, WebApplication> map)
        where TController : class
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(controller);
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        var app = builder.Build();
        map(app);
        await app.StartAsync();

        return new EndpointTestServer(app);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
