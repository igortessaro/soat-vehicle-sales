using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Register;

public sealed record RegisterPaymentGatewayCommand(string Name, string BaseUrl) : IRequest<Response<VehicleSales.Domain.Entities.PaymentGateway>>;
