using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System;

namespace Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Update;

public sealed record UpdatePaymentGatewayCommand(Guid Id, string Name, string BaseUrl) : IRequest<Response<VehicleSales.Domain.Entities.PaymentGateway>>;
