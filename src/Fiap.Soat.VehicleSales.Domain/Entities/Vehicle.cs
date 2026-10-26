using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Domain.Entities;

public sealed class Vehicle : Entity
{
    private Vehicle() { }

    public Vehicle(VehicleSnapshot snapshot) : base(snapshot.Id)
    {
        Apply(snapshot);
    }

    public string Brand { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string LicensePlate { get; private set; } = string.Empty;
    public int ManufactureYear { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public VehicleStatus Status { get; private set; }
    public DateTime SourceCreatedAt { get; private set; }
    public DateTime SourceUpdatedAt { get; private set; }

    public bool ApplySnapshot(VehicleSnapshot snapshot)
    {
        if (snapshot.Id != Id) throw new ArgumentException($"Snapshot of vehicle {snapshot.Id} cannot be applied to vehicle {Id}", nameof(snapshot));
        if (snapshot.UpdatedAt <= SourceUpdatedAt) return false;

        Apply(snapshot);
        MarkAsUpdated();
        return true;
    }

    private void Apply(VehicleSnapshot snapshot)
    {
        Brand = snapshot.Brand;
        Model = snapshot.Model;
        LicensePlate = snapshot.LicensePlate;
        ManufactureYear = snapshot.ManufactureYear;
        Color = snapshot.Color;
        Price = snapshot.Price;
        Status = snapshot.Status;
        SourceCreatedAt = snapshot.CreatedAt;
        SourceUpdatedAt = snapshot.UpdatedAt;
    }
}
