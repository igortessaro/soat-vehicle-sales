using Fiap.Soat.VehicleSales.Api.Shared.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class DetailedHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_ReturnsHealthyWithOperationalData()
    {
        var result = await new DetailedHealthCheck().CheckHealthAsync(new HealthCheckContext());

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(result.Description, Is.EqualTo("API is healthy"));
            Assert.That(result.Data, Does.ContainKey("version"));
            Assert.That(result.Data, Does.ContainKey("timestamp"));
            Assert.That(result.Data, Does.ContainKey("uptime"));
            Assert.That(result.Data, Does.ContainKey("memoryUsedMB"));
            Assert.That(result.Data, Does.ContainKey("gen0Collections"));
            Assert.That(result.Data, Does.ContainKey("gen1Collections"));
            Assert.That(result.Data, Does.ContainKey("gen2Collections"));
        });
    }
}
