using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Fiap.Soat.VehicleSales.Infrastructure.Repositories;

public sealed class SaleRepository(AppDbContext appDbContext) : Repository<Sale>(appDbContext), ISaleRepository
{
    public Task<Sale?> GetByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(s => s.PaymentCode == paymentCode, cancellationToken);
}
