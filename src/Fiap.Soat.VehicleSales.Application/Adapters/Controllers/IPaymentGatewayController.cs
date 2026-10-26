using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public interface IPaymentGatewayController
{
    Task<Response<PaymentGatewayDetail>> RegisterAsync(string name, string baseUrl, CancellationToken cancellationToken);
    Task<Response<PaymentGatewayDetail>> GetAsync(CancellationToken cancellationToken);
    Task<Response<PaymentGatewayDetail>> UpdateAsync(Guid id, string name, string baseUrl, CancellationToken cancellationToken);
}
