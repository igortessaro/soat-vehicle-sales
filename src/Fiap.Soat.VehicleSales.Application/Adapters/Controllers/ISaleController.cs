using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Domain.Shared;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public interface ISaleController
{
    Task<Response<SaleDetail>> CreateAsync(CreateSaleCommand request, CancellationToken cancellationToken);
}
