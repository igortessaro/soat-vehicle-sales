using Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;
using Fiap.Soat.VehicleSales.Domain.Shared;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public interface IWebhookController
{
    Task<Response> ProcessAsync(PaymentCommand command, CancellationToken cancellationToken);
}
