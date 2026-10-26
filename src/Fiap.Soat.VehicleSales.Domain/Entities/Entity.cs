namespace Fiap.Soat.VehicleSales.Domain.Entities;

public class Entity
{
    public Guid Id { get; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;
}
