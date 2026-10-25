using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Infrastructure.Services;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<IVehicleManagementService, VehicleManagementService>();
        _ = serviceCollection.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        return serviceCollection;
    }
}
