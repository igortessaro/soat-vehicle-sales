using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Application.Tests;

internal static class ApplicationTestData
{
    public const string ValidDocument = "52998224725";

    public static Sale CreateSale(SaleStatus status = SaleStatus.PendingPayment, Guid? vehicleId = null)
    {
        var sale = new Sale(vehicleId ?? Guid.NewGuid(), ValidDocument);

        return status switch
        {
            SaleStatus.PendingPayment => sale,
            SaleStatus.Paid => sale.Paid(),
            SaleStatus.Canceled => sale.Canceled(),
            SaleStatus.Completed => sale.Paid().Deliver(),
            _ => sale
        };
    }

    public static PaymentGateway CreatePaymentGateway() =>
        new("Gateway", "https://gateway.com");

    public static VehicleDetail CreateVehicleDetail(string status = "Available") =>
        new(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", 100000m, status, DateTime.UtcNow, DateTime.UtcNow);
}
