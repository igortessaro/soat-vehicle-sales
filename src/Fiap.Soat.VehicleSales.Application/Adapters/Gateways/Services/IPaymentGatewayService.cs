using Fiap.Soat.VehicleSales.Domain.Shared;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;

public interface IPaymentGatewayService
{
    Task<Response> CreatePaymentAsync(string paymentCode, CancellationToken cancellationToken = default);
}
