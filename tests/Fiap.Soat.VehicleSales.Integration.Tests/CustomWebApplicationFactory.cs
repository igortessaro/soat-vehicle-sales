using Bogus;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fiap.Soat.VehicleSales.Integration.Tests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected const string VehicleManagementUrl = "http://vehicle-management";
    protected const string PaymentGatewayUrl = "http://payment-gateway";

    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly HttpClient Client;
    protected readonly Faker Faker = new("pt_BR");
    protected readonly HttpStub HttpStub = new();
    private readonly string _databaseName = $"IntegrationTests-{Guid.NewGuid()}";

    protected CustomWebApplicationFactory()
    {
        Client = CreateClient();
    }

    [SetUp]
    public void ResetHttpStub() => HttpStub.Reset();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs requires a MySQL connection string at startup; it is never used because the DbContext is replaced below.
        builder.UseSetting("ConnectionStrings:MySql", "Server=localhost;Database=integration_tests;User=test;Password=test");
        builder.UseSetting("VehicleManagement:BaseUrl", VehicleManagementUrl);
        builder.ConfigureServices(services =>
        {
            // EF Core 9 keeps the provider configuration in IDbContextOptionsConfiguration, so both registrations must go.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            services.AddHttpClient(Options.DefaultName).ConfigurePrimaryHttpMessageHandler(() => HttpStub);
        });

        builder.UseEnvironment("IntegrationTest");
    }

    protected async Task<TEntity> SeedAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Set<TEntity>().AddAsync(entity);
        if (entity is PaymentGateway)
        {
            // The InMemory provider ignores HasDefaultValue, so the required shadow key must be set explicitly.
            dbContext.Entry(entity).Property("singleton_key").CurrentValue = "X";
        }

        await dbContext.SaveChangesAsync();
        return entity;
    }

    protected Task<PaymentGateway> SeedPaymentGatewayAsync() =>
        SeedAsync(new PaymentGateway("Integration Gateway", PaymentGatewayUrl));

    protected async Task<TEntity?> FindAsync<TEntity>(Guid id) where TEntity : class
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Set<TEntity>().FindAsync(id);
    }

    protected async Task<List<TEntity>> ListAsync<TEntity>() where TEntity : class
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Set<TEntity>().ToListAsync();
    }

    protected static async Task<Response<T>> ReadResponseAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<Response<T>>(JsonOptions))!;

    protected static async Task<ErrorResponse> ReadErrorAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions))!;
}
