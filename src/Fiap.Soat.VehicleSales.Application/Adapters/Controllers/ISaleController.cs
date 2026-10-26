using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Domain.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public interface ISaleController
{
    Task<Response<SaleDetail>> CreateAsync(CreateSaleCommand request, CancellationToken cancellationToken);
    Task<Response<SaleDetail>> DeliverAsync(Guid id, CancellationToken cancellationToken);
}
