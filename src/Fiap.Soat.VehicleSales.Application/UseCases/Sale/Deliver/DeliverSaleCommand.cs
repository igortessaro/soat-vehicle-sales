using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Sale.Deliver;

public record DeliverSaleCommand(Guid SaleId) : IRequest<Response<Domain.Entities.Sale>>;
