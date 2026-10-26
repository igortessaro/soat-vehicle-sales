using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Entities;

public sealed class Sale : Entity
{
    private Sale() { }

    public Sale(Guid vehicleId, string buyerDocument)
    {
        VehicleId = vehicleId;
        BuyerDocument = buyerDocument;
        PaymentCode = Guid.NewGuid().ToString("N");
    }

    public Guid VehicleId { get; private set; }
    public Document BuyerDocument { get; private set; } = string.Empty;
    public SaleStatus Status { get; private set; } = SaleStatus.PendingPayment;
    public string PaymentCode { get; private set; } = string.Empty;
    public DateTime? PaidAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    public Sale Paid()
    {
        Status = SaleStatus.Paid;
        PaidAt = DateTime.UtcNow;
        MarkAsUpdated();
        return this;
    }

    public Sale Canceled()
    {
        Status = SaleStatus.Canceled;
        CanceledAt = DateTime.UtcNow;
        MarkAsUpdated();
        return this;
    }

    public Sale Deliver()
    {
        Status = SaleStatus.Completed;
        MarkAsUpdated();
        return this;
    }
}
