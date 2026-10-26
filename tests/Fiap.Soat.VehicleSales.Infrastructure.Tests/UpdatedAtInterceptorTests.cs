using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Data;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class UpdatedAtInterceptorTests
{
    private static readonly DateTime StaleTimestamp = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task SaveChangesAsync_WhenEntityIsAdded_KeepsOriginalUpdatedAt()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var entity = new Sale(Guid.NewGuid(), InfrastructureTestFixture.ValidDocument);
        context.Sales.Add(entity);
        context.Entry(entity).Property(x => x.UpdatedAt).CurrentValue = StaleTimestamp;

        await context.SaveChangesAsync();

        Assert.That(entity.UpdatedAt, Is.EqualTo(StaleTimestamp));
    }

    [Test]
    public async Task SaveChangesAsync_WhenEntityIsModified_StampsUpdatedAt()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var entity = await AddWithStaleTimestampAsync(context);
        var beforeUpdate = DateTime.UtcNow;

        entity.Paid();
        context.Sales.Update(entity);
        await context.SaveChangesAsync();

        Assert.That(entity.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
    }

    [Test]
    public async Task SaveChanges_WhenEntityIsModified_StampsUpdatedAt()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var entity = await AddWithStaleTimestampAsync(context);
        var beforeUpdate = DateTime.UtcNow;

        entity.Paid();
        context.Sales.Update(entity);
        context.SaveChanges();

        Assert.That(entity.UpdatedAt, Is.GreaterThanOrEqualTo(beforeUpdate));
    }

    [Test]
    public async Task SaveChangesAsync_WhenEntityIsUnchanged_KeepsUpdatedAt()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var entity = await AddWithStaleTimestampAsync(context);

        await context.SaveChangesAsync();

        Assert.That(entity.UpdatedAt, Is.EqualTo(StaleTimestamp));
    }

    private static async Task<Sale> AddWithStaleTimestampAsync(AppDbContext context)
    {
        var entity = new Sale(Guid.NewGuid(), InfrastructureTestFixture.ValidDocument);
        context.Sales.Add(entity);
        context.Entry(entity).Property(x => x.UpdatedAt).CurrentValue = StaleTimestamp;
        await context.SaveChangesAsync();
        return entity;
    }
}
