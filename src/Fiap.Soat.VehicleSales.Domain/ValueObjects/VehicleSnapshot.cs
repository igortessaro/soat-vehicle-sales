namespace Fiap.Soat.VehicleSales.Domain.ValueObjects;

public sealed record VehicleSnapshot(
    Guid Id,
    string Brand,
    string Model,
    string LicensePlate,
    int ManufactureYear,
    string Color,
    decimal Price,
    VehicleStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
