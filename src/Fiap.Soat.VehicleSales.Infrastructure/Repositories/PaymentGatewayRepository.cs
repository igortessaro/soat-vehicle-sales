using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Infrastructure.Repositories;

public sealed class PaymentGatewayRepository(AppDbContext context)
    : Repository<PaymentGateway>(context), IPaymentGatewayRepository
{
    public Task<PaymentGateway?> GetRegisteredAsync(CancellationToken cancellationToken) =>
        context.PaymentGateways.OrderBy(gateway => gateway.CreatedAt).FirstOrDefaultAsync(cancellationToken);
}
