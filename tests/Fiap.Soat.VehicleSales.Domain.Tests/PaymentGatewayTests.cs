using Fiap.Soat.VehicleSales.Domain.Entities;

namespace Fiap.Soat.VehicleSales.Domain.Tests;

public class PaymentGatewayTests
{
    [TestCase("https://gateway.com", "https://gateway.com")]
    [TestCase("https://gateway.com/", "https://gateway.com")]
    [TestCase("https://gateway.com/api//", "https://gateway.com/api")]
    public void Constructor_ShouldInitializeValuesAndTrimTrailingSlashes(string baseUrl, string expectedBaseUrl)
    {
        var gateway = new PaymentGateway("Mercado Pago", baseUrl);

        Assert.Multiple(() =>
        {
            Assert.That(gateway.Id, Is.EqualTo(Guid.Empty));
            Assert.That(gateway.Name, Is.EqualTo("Mercado Pago"));
            Assert.That(gateway.BaseUrl, Is.EqualTo(expectedBaseUrl));
        });
    }

    [Test]
    public void Update_ShouldReplaceValuesAndReturnSameGateway()
    {
        var gateway = new PaymentGateway("Mercado Pago", "https://gateway.com");

        var result = gateway.Update("PagSeguro", "https://pagseguro.com/");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(gateway));
            Assert.That(gateway.Name, Is.EqualTo("PagSeguro"));
            Assert.That(gateway.BaseUrl, Is.EqualTo("https://pagseguro.com"));
        });
    }
}
