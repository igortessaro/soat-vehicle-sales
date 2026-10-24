using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;

public sealed class DeliverSaleHandler(ISaleRepository repository, IVehicleManagementService vehicleManagementService)
    : IRequestHandler<DeliverSaleCommand, Response<VehicleSales.Domain.Entities.Sale>>
{
    public async Task<Response<Domain.Entities.Sale>> Handle(DeliverSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null) return Response<Domain.Entities.Sale>.Fail($"Sale with id {request.SaleId} not found");

        var vehicleResponse = await vehicleManagementService.ReserveVehicleAsync(sale.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return Response<Domain.Entities.Sale>.Fail(vehicleResponse.Reasons[0].Message);

        await repository.UpdateAsync(sale.Deliver(), cancellationToken);
        return ResponseFactory.Ok(sale);
    }
}
