using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Fiap.Soat.VehicleSales.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Soat.VehicleSales.Infrastructure.Tests;

public class SaleRepositoryTests
{
    [Test]
    public async Task AddAndGetByIdAsync_PersistsSale()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var sale = CreateSale();

        var added = await repository.AddAsync(sale, CancellationToken.None);
        context.ChangeTracker.Clear();
        var result = await repository.GetByIdAsync(sale.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(added, Is.SameAs(sale));
            Assert.That(result!.Id, Is.EqualTo(sale.Id));
            Assert.That(result.BuyerDocument.Value, Is.EqualTo(InfrastructureTestFixture.ValidDocument));
            Assert.That(result.PaymentCode, Is.EqualTo(sale.PaymentCode));
        });
    }

    [Test]
    public async Task GetDetailedByIdAsync_ReturnsDetachedSale()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var sale = await AddSale(context);

        var result = await repository.GetDetailedByIdAsync(sale.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(context.Entry(result!).State, Is.EqualTo(EntityState.Detached));
        });
    }

    [Test]
    public async Task GetByPaymentCodeAsync_ReturnsMatchingSaleOrNull()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var sale = await AddSale(context);
        await AddSale(context);

        var found = await repository.GetByPaymentCodeAsync(sale.PaymentCode, CancellationToken.None);
        var missing = await repository.GetByPaymentCodeAsync("not-found", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(found!.Id, Is.EqualTo(sale.Id));
            Assert.That(context.Entry(found).State, Is.EqualTo(EntityState.Detached));
            Assert.That(missing, Is.Null);
        });
    }

    [Test]
    public async Task UpdateAsync_UpdatesSale()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var sale = await AddSale(context);

        var updated = await repository.UpdateAsync(sale.Paid(), CancellationToken.None);
        context.ChangeTracker.Clear();
        var result = await repository.GetByIdAsync(sale.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.SameAs(sale));
            Assert.That(result!.Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(result.PaidAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task DeleteAndDeleteRangeAsync_RemoveSales()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var first = await AddSale(context);
        var second = await AddSale(context);

        await repository.DeleteAsync(first, CancellationToken.None);
        await repository.DeleteRangeAsync([second], CancellationToken.None);

        Assert.That(await repository.AnyAsync(_ => true, CancellationToken.None), Is.False);
    }

    [Test]
    public async Task FindAndAnyAsync_ReturnExpectedResults()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var paid = await AddSale(context, SaleStatus.Paid);
        await AddSale(context);

        var found = await repository.FindAsync(x => x.Status == SaleStatus.Paid, CancellationToken.None);
        var single = await repository.FindSingleAsync(x => x.Id == paid.Id, CancellationToken.None);
        var hasCanceled = await repository.AnyAsync(x => x.Status == SaleStatus.Canceled, CancellationToken.None);
        var hasPending = await repository.AnyAsync(x => x.Status == SaleStatus.PendingPayment, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(found.Single().Id, Is.EqualTo(paid.Id));
            Assert.That(single!.Id, Is.EqualTo(paid.Id));
            Assert.That(hasCanceled, Is.False);
            Assert.That(hasPending, Is.True);
        });
    }

    [Test]
    public async Task GetAllPaginatedAsync_ReturnsMetadataAndPage()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        await AddSale(context);
        await AddSale(context, SaleStatus.Paid);
        var canceled = await AddSale(context, SaleStatus.Canceled);

        var result = await repository.GetAllAsync(
            new PaginatedRequest { PageNumber = 2, PageSize = 2 },
            CancellationToken.None,
            query => query.OrderBy(x => x.Status));

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(3));
            Assert.That(result.TotalPages, Is.EqualTo(2));
            Assert.That(result.CurrentPage, Is.EqualTo(2));
            Assert.That(result.Items.Single().Id, Is.EqualTo(canceled.Id));
        });
    }

    [Test]
    public async Task GetAllPaginatedAsync_WithoutOrderBy_OrdersByCreatedAt()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        var first = await AddSale(context);
        await AddSale(context);

        var result = await repository.GetAllAsync(
            new PaginatedRequest { PageNumber = 1, PageSize = 1 }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Items.Single().Id, Is.EqualTo(first.Id));
        });
    }

    [Test]
    public async Task GetAllPaginatedAsync_WhenPageIsZero_ReturnsNoItems()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        await AddSale(context);

        var result = await repository.GetAllAsync(
            new PaginatedRequest { PageNumber = 0, PageSize = 10 }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(1));
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.TotalPages, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task GetAllPaginatedAsync_SupportsPredicateAndIncludesOverloads()
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);
        await AddSale(context);
        await AddSale(context, SaleStatus.Paid);
        var request = new PaginatedRequest { PageNumber = 1, PageSize = 10 };

        var byPredicate = await repository.GetAllAsync(
            x => x.Status == SaleStatus.Paid, request, CancellationToken.None);
        var byIncludes = await repository.GetAllAsync(
            Array.Empty<string>(), request, CancellationToken.None);
        var byIncludesAndPredicate = await repository.GetAllAsync(
            Array.Empty<string>(), x => x.Status == SaleStatus.PendingPayment, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(byPredicate.Items.Single().Status, Is.EqualTo(SaleStatus.Paid));
            Assert.That(byIncludes.TotalCount, Is.EqualTo(2));
            Assert.That(byIncludesAndPredicate.Items.Single().Status, Is.EqualTo(SaleStatus.PendingPayment));
        });
    }

    [TestCase(true, EntityState.Detached)]
    [TestCase(false, EntityState.Unchanged)]
    public async Task Query_RespectsTrackingOption(bool noTracking, EntityState expectedState)
    {
        await using var context = InfrastructureTestFixture.CreateContext();
        var sale = await AddSale(context);
        context.ChangeTracker.Clear();
        var repository = new TestSaleRepository(context);

        var result = await repository.QueryById(sale.Id, noTracking);

        Assert.That(context.Entry(result!).State, Is.EqualTo(expectedState));
    }

    [Test]
    public void GetDbConnection_ThrowsForNonRelationalProvider()
    {
        using var context = InfrastructureTestFixture.CreateContext();
        var repository = new SaleRepository(context);

        Assert.That(
            () => repository.GetDbConnection(),
            Throws.InvalidOperationException);
    }

    private static Sale CreateSale(SaleStatus status = SaleStatus.PendingPayment)
    {
        var sale = new Sale(Guid.NewGuid(), InfrastructureTestFixture.ValidDocument);
        return status switch
        {
            SaleStatus.Paid => sale.Paid(),
            SaleStatus.Canceled => sale.Canceled(),
            SaleStatus.Completed => sale.Paid().Deliver(),
            _ => sale
        };
    }

    private static async Task<Sale> AddSale(AppDbContext context, SaleStatus status = SaleStatus.PendingPayment)
    {
        var sale = CreateSale(status);
        context.Sales.Add(sale);
        await context.SaveChangesAsync();
        return sale;
    }

    private sealed class TestSaleRepository(AppDbContext context) : Repository<Sale>(context)
    {
        public Task<Sale?> QueryById(Guid id, bool noTracking) =>
            Query(noTracking).FirstOrDefaultAsync(x => x.Id == id);
    }
}
