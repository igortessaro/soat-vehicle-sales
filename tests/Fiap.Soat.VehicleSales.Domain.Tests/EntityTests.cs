using Fiap.Soat.VehicleSales.Domain.Entities;

namespace Fiap.Soat.VehicleSales.Domain.Tests;

public class EntityTests
{
    [Test]
    public void Constructor_ShouldInitializeIdentityAndTimestamps()
    {
        var beforeCreation = DateTime.UtcNow;

        var entity = new Entity();

        var afterCreation = DateTime.UtcNow;
        Assert.Multiple(() =>
        {
            Assert.That(entity.Id, Is.EqualTo(Guid.Empty));
            Assert.That(entity.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
            Assert.That(entity.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
            Assert.That(entity.UpdatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
            Assert.That(entity.UpdatedAt, Is.LessThanOrEqualTo(afterCreation));
        });
    }
}
