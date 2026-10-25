using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Get;

public sealed record GetPaymentGatewayQuery : IRequest<Response<VehicleSales.Domain.Entities.PaymentGateway>>;
