using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Application.Tests;

public class VehicleMapperTests
{
    [Test]
    public void ToSnapshot_ShouldCopyEveryField()
    {
        var createdAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        var vehicle = new VehicleDetail(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", 120000m, "Reserved", createdAt, updatedAt);

        var snapshot = vehicle.ToSnapshot();

        Assert.That(snapshot, Is.EqualTo(new VehicleSnapshot(
            vehicle.Id, "Toyota", "Corolla", "ABC1D23", 2024, "Black", 120000m, VehicleStatus.Reserved, createdAt, updatedAt)));
    }

    [TestCase("Available", VehicleStatus.Available)]
    [TestCase("sold", VehicleStatus.Sold)]
    [TestCase("DELIVERED", VehicleStatus.Delivered)]
    public void ToSnapshot_ShouldParseStatusIgnoringCase(string status, VehicleStatus expected)
    {
        var snapshot = ApplicationTestData.CreateVehicleDetail(status).ToSnapshot();

        Assert.That(snapshot.Status, Is.EqualTo(expected));
    }

    [Test]
    public void ToSnapshot_WhenStatusIsUnknown_Throws()
    {
        var vehicle = ApplicationTestData.CreateVehicleDetail("Scrapped");

        Assert.That(() => vehicle.ToSnapshot(), Throws.ArgumentException);
    }
}
