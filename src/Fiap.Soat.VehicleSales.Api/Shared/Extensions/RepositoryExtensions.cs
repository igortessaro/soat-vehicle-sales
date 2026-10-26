using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.VehicleSales.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryExtensions(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<ISaleRepository, SaleRepository>();
        _ = serviceCollection.AddScoped<IPaymentGatewayRepository, PaymentGatewayRepository>();

        return serviceCollection;
    }
}
