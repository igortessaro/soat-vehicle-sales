using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Infrastructure.Data;

namespace Fiap.Soat.VehicleSales.Infrastructure.Repositories;

public sealed class SaleRepository(AppDbContext appDbContext) : Repository<Sale>(appDbContext), ISaleRepository
{
}
