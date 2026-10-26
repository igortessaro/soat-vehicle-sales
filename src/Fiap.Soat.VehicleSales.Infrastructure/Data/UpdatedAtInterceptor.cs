using Fiap.Soat.VehicleSales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fiap.Soat.VehicleSales.Infrastructure.Data;

/// <summary>
/// Stamps <see cref="Entity.UpdatedAt"/> on every modified entity. The database default
/// (ON UPDATE CURRENT_TIMESTAMP) never fires because EF Core always sends updated_at explicitly.
/// </summary>
public sealed class UpdatedAtInterceptor : SaveChangesInterceptor
{
    public static readonly UpdatedAtInterceptor Instance = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        StampModifiedEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        StampModifiedEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void StampModifiedEntities(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries<Entity>().Where(entry => entry.State == EntityState.Modified))
        {
            entry.Property(entity => entity.UpdatedAt).CurrentValue = now;
        }
    }
}
