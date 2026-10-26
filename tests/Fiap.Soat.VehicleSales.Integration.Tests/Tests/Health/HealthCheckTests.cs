using System.Net;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Tests.Health;

public sealed class HealthCheckTests : CustomWebApplicationFactory<Program>
{
    [Test]
    public async Task Health_GetAsync_ShouldReportApiAndDatabaseAsHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Does.Contain("\"status\":\"Healthy\""));
            Assert.That(content, Does.Contain("\"detailed\""));
            Assert.That(content, Does.Contain("\"database\""));
        });
    }
}
