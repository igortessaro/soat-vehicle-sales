using Fiap.Soat.VehicleSales.Api.Shared.Filters;
using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Soat.VehicleSales.Api.Endpoints;

public static class SaleEndpoints
{
    public static WebApplication Map(WebApplication app)
    {
        var salesApi = app
            .MapGroup("api/v1/sales")
            .AddEndpointFilter<ResponseFilter<SaleDetail>>()
            .WithTags("Sales");

        salesApi.MapPost("", ([FromServices] ISaleController controller, [FromBody] CreateSaleCommand command, CancellationToken cancellationToken) =>
                controller.CreateAsync(command, cancellationToken))
            .WithName("CreateSale")
            .WithDescription("Create a new sale");

        return app;
    }
}
