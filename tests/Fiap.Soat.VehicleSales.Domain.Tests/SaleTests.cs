using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Tests;

public class SaleTests
{
    [Test]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        var vehicleId = Guid.NewGuid();

        var sale = new Sale(vehicleId, "52998224725");

        Assert.Multiple(() =>
        {
            Assert.That(sale.Id, Is.EqualTo(Guid.Empty));
            Assert.That(sale.VehicleId, Is.EqualTo(vehicleId));
            Assert.That(sale.BuyerDocument.Value, Is.EqualTo("52998224725"));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.PendingPayment));
            Assert.That(sale.PaymentCode, Has.Length.EqualTo(32));
            Assert.That(sale.PaidAt, Is.Null);
            Assert.That(sale.CanceledAt, Is.Null);
        });
    }

    [Test]
    public void Constructor_ShouldGenerateUniquePaymentCodes()
    {
        var first = CreateSale();
        var second = CreateSale();

        Assert.That(first.PaymentCode, Is.Not.EqualTo(second.PaymentCode));
    }

    [Test]
    public void Paid_ShouldSetStatusAndPaidAtAndReturnSameSale()
    {
        var sale = CreateSale();
        var beforePayment = DateTime.UtcNow;

        var result = sale.Paid();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(sale));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(sale.PaidAt, Is.Not.Null.And.GreaterThanOrEqualTo(beforePayment));
            Assert.That(sale.CanceledAt, Is.Null);
        });
    }

    [Test]
    public void Canceled_ShouldSetStatusAndCanceledAtAndReturnSameSale()
    {
        var sale = CreateSale();
        var beforeCancellation = DateTime.UtcNow;

        var result = sale.Canceled();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(sale));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Canceled));
            Assert.That(sale.CanceledAt, Is.Not.Null.And.GreaterThanOrEqualTo(beforeCancellation));
            Assert.That(sale.PaidAt, Is.Null);
        });
    }

    [Test]
    public void Deliver_ShouldSetCompletedStatusAndReturnSameSale()
    {
        var sale = CreateSale().Paid();

        var result = sale.Deliver();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(sale));
            Assert.That(sale.Status, Is.EqualTo(SaleStatus.Completed));
        });
    }

    private static Sale CreateSale() => new(Guid.NewGuid(), "52998224725");
}
