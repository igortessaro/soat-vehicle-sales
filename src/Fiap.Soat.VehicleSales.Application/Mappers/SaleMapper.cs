using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

namespace Fiap.Soat.VehicleSales.Application.Mappers;

public static class SaleMapper
{
    public static VehicleSales.Domain.Entities.Sale ToDomainEntity(this CreateSaleCommand command) =>
        new VehicleSales.Domain.Entities.Sale(command.VehicleId, command.BuyerDocument);
}
