using Bogus;
using Bogus.Extensions.Brazil;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Factories;

public static class SaleFactory
{
    private static readonly Faker Faker = new("pt_BR");

    public static string CreateValidCpf() => Faker.Person.Cpf(includeFormatSymbols: false);

    public static Sale CreateSale(SaleStatus status = SaleStatus.PendingPayment, Guid? vehicleId = null)
    {
        var sale = new Sale(vehicleId ?? Guid.NewGuid(), CreateValidCpf());

        return status switch
        {
            SaleStatus.Paid => sale.Paid(),
            SaleStatus.Canceled => sale.Canceled(),
            SaleStatus.Completed => sale.Paid().Deliver(),
            _ => sale
        };
    }
}
