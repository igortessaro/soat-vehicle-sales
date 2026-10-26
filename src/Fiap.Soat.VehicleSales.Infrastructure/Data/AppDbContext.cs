using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    public DbSet<Sale> Sales { get; set; }
    public DbSet<PaymentGateway> PaymentGateways { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SaleConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentGatewayConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
}
