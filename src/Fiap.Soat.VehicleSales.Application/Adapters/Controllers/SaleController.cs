using Fiap.Soat.VehicleSales.Application.Adapters.Presenters;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public sealed class SaleController(IMediator mediator) : ISaleController
{
    public async Task<Response<SaleDetail>> CreateAsync(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        var result = ResponseMapper.Map(response, SalePresenter.ToDto);
        return result;
    }

    public async Task<Response<SaleDetail>> DeliverAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new DeliverSaleCommand(id), cancellationToken);
        var result = ResponseMapper.Map(response, SalePresenter.ToDto);
        return result;
    }
}
