using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Webhook.Payment;

public record PaymentPaidCommand(string PaymentCode) : IRequest<Response>;
