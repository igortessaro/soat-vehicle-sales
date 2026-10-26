using Fiap.Soat.VehicleSales.Application.Adapters.Presenters;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Get;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Register;
using Fiap.Soat.VehicleSales.Application.UseCases.PaymentGateway.Update;
using Fiap.Soat.VehicleSales.Domain.Shared;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Controllers;

public sealed class PaymentGatewayController(IMediator mediator) : IPaymentGatewayController
{
    public async Task<Response<PaymentGatewayDetail>> RegisterAsync(string name, string baseUrl, CancellationToken cancellationToken) =>
        Map(await mediator.Send(new RegisterPaymentGatewayCommand(name, baseUrl), cancellationToken));

    public async Task<Response<PaymentGatewayDetail>> GetAsync(CancellationToken cancellationToken) =>
        Map(await mediator.Send(new GetPaymentGatewayQuery(), cancellationToken));

    public async Task<Response<PaymentGatewayDetail>> UpdateAsync(Guid id, string name, string baseUrl, CancellationToken cancellationToken) =>
        Map(await mediator.Send(new UpdatePaymentGatewayCommand(id, name, baseUrl), cancellationToken));

    private static Response<PaymentGatewayDetail> Map(Response<VehicleSales.Domain.Entities.PaymentGateway> response) =>
        response.IsSuccess
            ? ResponseFactory.Ok(PaymentGatewayPresenter.ToDto(response.Data), response.StatusCode)
            : ResponseFactory.Fail<PaymentGatewayDetail>(string.Join(", ", response.Reasons.Select(reason => reason.Message)), response.StatusCode);
}
