using Fiap.Soat.VehicleSales.Domain.Entities;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;

public interface IPaymentGatewayRepository : IRepository<PaymentGateway>
{
    Task<PaymentGateway?> GetRegisteredAsync(CancellationToken cancellationToken);
}
