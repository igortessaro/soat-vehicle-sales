namespace Fiap.Soat.VehicleSales.Application.Models;

public record VehicleDetail(Guid Id, string Brand, string Model, string LicensePlate, int Year, string Color, decimal Price, string Status, DateTime CreatedAt, DateTime UpdatedAt);
