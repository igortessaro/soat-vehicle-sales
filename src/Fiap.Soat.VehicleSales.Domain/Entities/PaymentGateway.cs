namespace Fiap.Soat.VehicleSales.Domain.Entities;

public sealed class PaymentGateway : Entity
{
    private PaymentGateway() { }

    public PaymentGateway(string name, string baseUrl)
    {
        Name = name;
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public string Name { get; private set; } = string.Empty;
    public string BaseUrl { get; private set; } = string.Empty;

    public PaymentGateway Update(string name, string baseUrl)
    {
        Name = name;
        BaseUrl = baseUrl.TrimEnd('/');
        MarkAsUpdated();
        return this;
    }
}
