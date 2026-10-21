using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Entities;

public sealed class Sale : Entity
{
    private Sale() { }

    public Sale(string vehicleId, string buyerDocument, string paymentCode)
    {
        VehicleId = vehicleId;
        BuyerDocument = buyerDocument;
        PaymentCode = paymentCode;
    }

    public string VehicleId { get; private set; }
    public string BuyerDocument { get; private set; }
    public SaleStatus Status { get; private set; } = SaleStatus.PendingPayment;
    public string PaymentCode { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }
}
