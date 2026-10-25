using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Repositories;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class PaymentGatewayRepositoryTests
{
    [Test]
    public async Task GetRegisteredAsync_WhenNoGatewayExists_ReturnsNull()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new PaymentGatewayRepository(context);

        var result = await repository.GetRegisteredAsync(CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRegisteredAsync_ReturnsRegisteredGateway()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new PaymentGatewayRepository(context);
        var gateway = new PaymentGateway("Gateway", "https://gateway.com/");
        context.PaymentGateways.Add(gateway);
        // The InMemory provider ignores HasDefaultValue, so the required shadow key must be set explicitly.
        context.Entry(gateway).Property("singleton_key").CurrentValue = "X";
        await context.SaveChangesAsync();

        var result = await repository.GetRegisteredAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Id, Is.EqualTo(gateway.Id));
            Assert.That(result.Name, Is.EqualTo("Gateway"));
            Assert.That(result.BaseUrl, Is.EqualTo("https://gateway.com"));
        });
    }
}
