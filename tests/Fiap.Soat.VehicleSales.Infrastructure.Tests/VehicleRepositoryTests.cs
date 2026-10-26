using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Fiap.Soat.VehicleSales.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class VehicleRepositoryTests
{
    private static readonly DateTime SourceUpdatedAt = new(2026, 1, 2, 10, 0, 0, DateTimeKind.Utc);

    [Test]
    public async Task UpsertAsync_WhenVehicleDoesNotExist_InsertsItWithTheSourceId()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        var snapshot = CreateSnapshot();

        await repository.UpsertAsync(snapshot, CancellationToken.None);
        context.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(snapshot.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(persisted, Is.Not.Null);
            Assert.That(persisted!.Brand, Is.EqualTo(snapshot.Brand));
            Assert.That(persisted.Price, Is.EqualTo(snapshot.Price));
            Assert.That(persisted.SourceUpdatedAt, Is.EqualTo(snapshot.UpdatedAt));
        });
    }

    [Test]
    public async Task UpsertAsync_WhenSnapshotIsNewer_UpdatesTheVehicle()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        var snapshot = CreateSnapshot();
        await repository.UpsertAsync(snapshot, CancellationToken.None);
        context.ChangeTracker.Clear();

        await repository.UpsertAsync(snapshot with { Status = VehicleStatus.Sold, UpdatedAt = SourceUpdatedAt.AddMinutes(1) }, CancellationToken.None);
        context.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(snapshot.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(persisted!.Status, Is.EqualTo(VehicleStatus.Sold));
            Assert.That(persisted.SourceUpdatedAt, Is.EqualTo(SourceUpdatedAt.AddMinutes(1)));
        });
    }

    [Test]
    public async Task UpsertAsync_WhenSnapshotIsOlder_KeepsTheCurrentData()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        var snapshot = CreateSnapshot();
        await repository.UpsertAsync(snapshot, CancellationToken.None);
        context.ChangeTracker.Clear();

        await repository.UpsertAsync(snapshot with { Price = 1m, UpdatedAt = SourceUpdatedAt.AddMinutes(-1) }, CancellationToken.None);
        context.ChangeTracker.Clear();
        var persisted = await repository.GetByIdAsync(snapshot.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(persisted!.Price, Is.EqualTo(snapshot.Price));
            Assert.That(persisted.SourceUpdatedAt, Is.EqualTo(SourceUpdatedAt));
        });
    }

    [Test]
    public async Task UpsertAsync_WhenTheSameSnapshotArrivesTwice_KeepsASingleVehicle()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        var snapshot = CreateSnapshot();

        await repository.UpsertAsync(snapshot, CancellationToken.None);
        await repository.UpsertAsync(snapshot, CancellationToken.None);

        Assert.That(context.Vehicles.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ListByStatusAsync_ReturnsOnlyRequestedStatusesOrderedByPrice()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        var expensiveSold = await repository.UpsertAsync(CreateSnapshot(VehicleStatus.Sold, 150000m), CancellationToken.None);
        var cheapDelivered = await repository.UpsertAsync(CreateSnapshot(VehicleStatus.Delivered, 60000m), CancellationToken.None);
        await repository.UpsertAsync(CreateSnapshot(VehicleStatus.Available, 50000m), CancellationToken.None);
        await repository.UpsertAsync(CreateSnapshot(VehicleStatus.Reserved, 70000m), CancellationToken.None);

        var result = await repository.ListByStatusAsync([VehicleStatus.Sold, VehicleStatus.Delivered], CancellationToken.None);

        Assert.That(result.Select(x => x.Id), Is.EqualTo(new[] { cheapDelivered.Id, expensiveSold.Id }));
    }

    [Test]
    public async Task GetLatestSourceUpdatedAtAsync_WhenThereAreNoVehicles_ReturnsNull()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);

        var result = await repository.GetLatestSourceUpdatedAtAsync(CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLatestSourceUpdatedAtAsync_ReturnsTheMostRecentSourceUpdate()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new VehicleRepository(context);
        await repository.UpsertAsync(CreateSnapshot() with { UpdatedAt = SourceUpdatedAt.AddHours(2) }, CancellationToken.None);
        await repository.UpsertAsync(CreateSnapshot(), CancellationToken.None);

        var result = await repository.GetLatestSourceUpdatedAtAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(SourceUpdatedAt.AddHours(2)));
    }

    [Test]
    public async Task UpsertAsync_WhenSavingFails_DetachesTheVehicleSoTheNextSaveIsNotAffected()
    {
        var failOnce = new FailingSaveInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(failOnce)
            .Options;
        await using var context = new AppDbContext(options);
        var vehicles = new VehicleRepository(context);
        var sales = new SaleRepository(context);

        Assert.That(
            async () => await vehicles.UpsertAsync(CreateSnapshot(), CancellationToken.None),
            Throws.InstanceOf<DbUpdateException>());
        var sale = await sales.AddAsync(new Sale(Guid.NewGuid(), InfrastructureTestFixture.ValidDocument), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(context.ChangeTracker.Entries<Vehicle>(), Is.Empty);
            Assert.That(context.Sales.Single().Id, Is.EqualTo(sale.Id));
            Assert.That(context.Vehicles.Count(), Is.Zero);
        });
    }

    private static VehicleSnapshot CreateSnapshot(VehicleStatus status = VehicleStatus.Available, decimal price = 120000m) =>
        new(Guid.NewGuid(), "Toyota", "Corolla", "ABC1D23", 2024, "Black", price, status, SourceUpdatedAt.AddDays(-1), SourceUpdatedAt);

    private sealed class FailingSaveInterceptor : SaveChangesInterceptor
    {
        private bool _failed;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (_failed) return base.SavingChangesAsync(eventData, result, cancellationToken);

            _failed = true;
            throw new DbUpdateException("Duplicate entry");
        }
    }
}
