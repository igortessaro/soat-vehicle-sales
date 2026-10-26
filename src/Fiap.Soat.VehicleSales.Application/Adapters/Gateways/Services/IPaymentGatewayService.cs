using Fiap.Soat.VehicleSales.Domain.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;

public interface IPaymentGatewayService
{
    Task<Response> CreatePaymentAsync(string paymentCode, CancellationToken cancellationToken = default);
}
