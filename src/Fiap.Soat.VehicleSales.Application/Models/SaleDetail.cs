using Fiap.Soat.VehicleSales.Domain.ValueObjects;

namespace Fiap.Soat.VehicleSales.Application.Models;

public record SaleDetail(
    Guid Id,
    Guid VehicleId,
    string BuyerDocument,
    SaleStatus Status,
    string PaymentCode,
    DateTime? PaidAt,
    DateTime? CanceledAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
