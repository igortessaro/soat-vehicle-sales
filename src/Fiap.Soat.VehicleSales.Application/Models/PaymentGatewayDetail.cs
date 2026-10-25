namespace Fiap.Soat.VehicleSales.Application.Models;

public sealed record PaymentGatewayDetail(
    Guid Id,
    string Name,
    string BaseUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
