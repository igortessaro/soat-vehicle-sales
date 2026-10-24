using Fiap.Soat.VehicleSales.Api.Shared.Filters;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Soat.VehicleSales.Api.Endpoints;

public static class VehiclesEndpoints
{
    public static WebApplication Map(WebApplication app)
    {
        var vehiclesApi = app
            .MapGroup("api/v1/vehicles")
            .AddEndpointFilter<ResponseFilter<VehicleDetail>>()
            .WithTags("Vehicles");

        vehiclesApi.MapGet("available", ([FromServices] IVehicleController controller, CancellationToken cancellationToken) =>
            controller.GetAllAvailableVehicles(cancellationToken))
            .WithName("GetAllAvailableVehicles")
            .WithDescription("Get all available vehicles");

        vehiclesApi.MapGet("sold", ([FromServices] IVehicleController controller, CancellationToken cancellationToken) =>
            controller.GetAllSoldVehicles(cancellationToken))
            .WithName("GetAllSoldVehicles")
            .WithDescription("Get all sold vehicles");

        return app;
    }
}
