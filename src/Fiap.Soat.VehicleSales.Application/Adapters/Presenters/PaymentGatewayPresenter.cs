using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Presenters;

public static class PaymentGatewayPresenter
{
    public static PaymentGatewayDetail ToDto(PaymentGateway gateway) =>
        new(gateway.Id, gateway.Name, gateway.BaseUrl, gateway.CreatedAt, gateway.UpdatedAt);
}
