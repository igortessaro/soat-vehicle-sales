using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

public sealed class CreateSaleHandler(ISaleRepository repository, IVehicleManagementService vehicleManagementService)
    : IRequestHandler<CreateSaleCommand, Response<VehicleSales.Domain.Entities.Sale>>
{
    public async Task<Response<VehicleSales.Domain.Entities.Sale>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        if (request.VehicleId == Guid.Empty) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("VehicleId is required");
        if (string.IsNullOrEmpty(request.BuyerDocument)) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("BuyerDocument is required");

        var vehicleResponse = await vehicleManagementService.GetAsync(request.VehicleId, cancellationToken);
        if (!vehicleResponse.IsSuccess) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>(vehicleResponse.Reasons[0].Message);

        var entity = request.ToDomainEntity();
        if (!entity.BuyerDocument.IsValid()) ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>($"BuyerDocument {entity.BuyerDocument} is invalid");
        await vehicleManagementService.ReserveVehicleAsync(request.VehicleId, cancellationToken);
        var sale = await repository.AddAsync(entity, cancellationToken);
        return ResponseFactory.Ok(sale);
    }
}
