using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Application.Mappers;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;

public sealed class CreateVehicleHandler(ISaleRepository repository) : IRequestHandler<CreateSaleCommand, Response<VehicleSales.Domain.Entities.Sale>>
{
    public async Task<Response<VehicleSales.Domain.Entities.Sale>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.VehicleId)) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("VehicleId is required");
        if (string.IsNullOrEmpty(request.BuyerDocument)) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("BuyerDocument is required");
        if (string.IsNullOrEmpty(request.PaymentCode)) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Sale>("PaymentCode is required");

        // TODO: implment get vehicle by id and check if it exists
        // var existingVehicle = await repository.GetByLicensePlateAsync(request.LicensePlate, cancellationToken);
        // if (existingVehicle != null) return ResponseFactory.Fail<VehicleSales.Domain.Entities.Vehicle>("Vehicle with this license plate already exists");

        var entity = request.ToDomainEntity();
        return ResponseFactory.Ok(await repository.AddAsync(entity, cancellationToken));
    }
}
