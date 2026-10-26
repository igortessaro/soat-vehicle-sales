using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Tests;

public class VehicleTests
{
    private static readonly DateTime SourceCreatedAt = new(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SourceUpdatedAt = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Constructor_ShouldCopySnapshotAndKeepItsId()
    {
        var snapshot = CreateSnapshot();

        var vehicle = new Vehicle(snapshot);

        Assert.Multiple(() =>
        {
            Assert.That(vehicle.Id, Is.EqualTo(snapshot.Id));
            Assert.That(vehicle.Brand, Is.EqualTo("Toyota"));
            Assert.That(vehicle.Model, Is.EqualTo("Corolla"));
            Assert.That(vehicle.LicensePlate, Is.EqualTo("ABC1D23"));
            Assert.That(vehicle.ManufactureYear, Is.EqualTo(2024));
            Assert.That(vehicle.Color, Is.EqualTo("Black"));
            Assert.That(vehicle.Price, Is.EqualTo(120000m));
            Assert.That(vehicle.Status, Is.EqualTo(VehicleStatus.Available));
            Assert.That(vehicle.SourceCreatedAt, Is.EqualTo(SourceCreatedAt));
            Assert.That(vehicle.SourceUpdatedAt, Is.EqualTo(SourceUpdatedAt));
        });
    }

    [Test]
    public void ApplySnapshot_WhenSnapshotIsNewer_ShouldReplaceValuesAndMarkAsUpdated()
    {
        var snapshot = CreateSnapshot();
        var vehicle = new Vehicle(snapshot);
        var newer = snapshot with { Price = 99000m, Status = VehicleStatus.Sold, UpdatedAt = SourceUpdatedAt.AddMinutes(5) };
        var beforeUpdate = DateTime.UtcNow;

        var applied = vehicle.ApplySnapshot(newer);

        Assert.Multiple(() =>
        {
            Assert.That(applied, Is.True);
            Assert.That(vehicle.Price, Is.EqualTo(99000m));
            Assert.That(vehicle.Status, Is.EqualTo(VehicleStatus.Sold));
            Assert.That(vehicle.SourceUpdatedAt, Is.EqualTo(newer.UpdatedAt));
            Assert.That(vehicle.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
        });
    }

    [TestCase(0, TestName = "ApplySnapshot_WhenSnapshotIsTheSameVersion_ShouldBeIgnored")]
    [TestCase(-5, TestName = "ApplySnapshot_WhenSnapshotIsOlder_ShouldBeIgnored")]
    public void ApplySnapshot_WhenSnapshotIsNotNewer_ShouldBeIgnored(int minutesFromCurrentVersion)
    {
        var snapshot = CreateSnapshot();
        var vehicle = new Vehicle(snapshot);
        var updatedAt = vehicle.UpdatedAt;
        var stale = snapshot with { Price = 1m, Status = VehicleStatus.Delivered, UpdatedAt = SourceUpdatedAt.AddMinutes(minutesFromCurrentVersion) };

        var applied = vehicle.ApplySnapshot(stale);

        Assert.Multiple(() =>
        {
            Assert.That(applied, Is.False);
            Assert.That(vehicle.Price, Is.EqualTo(120000m));
            Assert.That(vehicle.Status, Is.EqualTo(VehicleStatus.Available));
            Assert.That(vehicle.SourceUpdatedAt, Is.EqualTo(SourceUpdatedAt));
            Assert.That(vehicle.UpdatedAt, Is.EqualTo(updatedAt));
        });
    }

    [Test]
    public void ApplySnapshot_WhenSnapshotBelongsToAnotherVehicle_ShouldThrow()
    {
        var vehicle = new Vehicle(CreateSnapshot());
        var otherVehicle = CreateSnapshot() with { UpdatedAt = SourceUpdatedAt.AddDays(1) };

        Assert.That(() => vehicle.ApplySnapshot(otherVehicle), Throws.ArgumentException.With.Message.Contains(otherVehicle.Id.ToString()));
    }

    private static VehicleSnapshot CreateSnapshot() =>
        new(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", 120000m, VehicleStatus.Available, SourceCreatedAt, SourceUpdatedAt);
}
