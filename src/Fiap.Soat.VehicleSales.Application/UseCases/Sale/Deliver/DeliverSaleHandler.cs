using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Copy;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;

public sealed class DeliverSaleHandler(
    ISaleRepository repository,
    IVehicleManagementService vehicleManagementService,
    IPublisher publisher)
    : IRequestHandler<DeliverSaleCommand, Response<VehicleSales.Domain.Entities.Sale>>
{
    public async Task<Response<Domain.Entities.Sale>> Handle(DeliverSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await repository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null) return Response<Domain.Entities.Sale>.Fail($"Sale with id {request.SaleId} not found");

        var vehicleResponse = await vehicleManagementService.DeliverVehicleAsync(sale.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return Response<Domain.Entities.Sale>.Fail(vehicleResponse.Reasons[0].Message);

        await repository.UpdateAsync(sale.Deliver(), cancellationToken);
        await publisher.Publish(new VehicleChangedNotification(vehicleResponse.Data), cancellationToken);
        return ResponseFactory.Ok(sale);
    }
}
