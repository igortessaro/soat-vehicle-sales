using Fiap.Soat.VehicleSales.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fiap.Soat.VehicleSales.Infrastructure.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.VehicleId)
            .HasColumnName("vehicle_id")
            .HasColumnType("CHAR(36)")
            .HasMaxLength(36)
            .IsRequired();

        builder.OwnsOne(x => x.BuyerDocument, document =>
        {
            document.Property(x => x.Value)
                .HasColumnName("buyer_document")
                .HasColumnType("VARCHAR(50)")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasColumnType("VARCHAR(30)")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PaymentCode)
            .HasColumnName("payment_code")
            .HasColumnType("VARCHAR(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("paid_at");

        builder.Property(x => x.CanceledAt)
            .HasColumnType("datetime(6)")
            .HasColumnName("canceled_at");
    }
}
