using Bogus;

namespace Fiap.Soat.VehicleSales.Integration.Tests.Factories;

/// <summary>
/// Builds payloads in the same shape the vehicle-management API returns.
/// </summary>
public static class VehicleManagementApiFactory
{
    private static readonly Faker Faker = new("pt_BR");

    public static object Vehicle(Guid id, string status = "Available") => new
    {
        Id = id,
        Brand = Faker.Vehicle.Manufacturer(),
        Model = Faker.Vehicle.Model(),
        LicensePlate = Faker.Random.Replace("???#?##").ToUpperInvariant(),
        Year = Faker.Random.Int(2015, DateTime.UtcNow.Year),
        Color = Faker.Commerce.Color(),
        Price = Faker.Random.Decimal(30000, 300000),
        Status = status,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public static object Success(object data) => new { Data = data, Reasons = Array.Empty<string>(), IsSuccess = true };
}
