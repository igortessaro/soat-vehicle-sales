using Fiap.Soat.VehicleSales.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken);
}
