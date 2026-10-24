using Fiap.Soat.VehicleSales.Application.Adapters.Controllers;
using Fiap.Soat.VehicleSales.Application.UseCases.Sale.Create;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Application.Shared;

[ExcludeFromCodeCoverage]
public static class Bootstrap
{
    public static IServiceCollection AddInterfaceAdapters(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<ISaleController, SaleController>();
        _ = serviceCollection.AddScoped<IVehicleController, VehicleController>();
        _ = serviceCollection.AddScoped<IWebhookController, WebhookController>();
        return serviceCollection;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSaleCommand).Assembly));
        return serviceCollection;
    }
}
