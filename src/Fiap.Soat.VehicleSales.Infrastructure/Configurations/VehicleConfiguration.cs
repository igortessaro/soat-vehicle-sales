using Fiap.Soat.VehicleSales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.Soat.VehicleSales.Infrastructure.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("CHAR(36)")
            .ValueGeneratedNever();

        builder.Property(x => x.Brand)
            .HasColumnName("brand")
            .HasColumnType("VARCHAR(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Model)
            .HasColumnName("model")
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LicensePlate)
            .HasColumnName("license_plate")
            .HasColumnType("VARCHAR(20)")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ManufactureYear)
            .HasColumnName("manufacture_year")
            .IsRequired();

        builder.Property(x => x.Color)
            .HasColumnName("color")
            .HasColumnType("VARCHAR(50)")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnName("price")
            .HasColumnType("DECIMAL(12,2)")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasColumnType("VARCHAR(30)")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SourceCreatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("source_created_at")
            .IsRequired();

        builder.Property(x => x.SourceUpdatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("source_updated_at")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
