using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Entities;
using Fiap.Soat.VehicleSales.Domain.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Fiap.Soat.VehicleSales.Application.Adapters.Presenters;

public static class SalePresenter
{
    public static SaleDetail ToDto(Sale entity) =>
        new SaleDetail(entity.Id, entity.VehicleId, entity.BuyerDocument, entity.Status, entity.PaymentCode, entity.PaidAt, entity.CanceledAt, entity.CreatedAt, entity.UpdatedAt);

    public static Response<SaleDetail> ToDto(Response<Sale> response) =>
        response.IsSuccess ?
            ResponseFactory.Ok(ToDto(response.Data)) :
            ResponseFactory.Fail<SaleDetail>(response.Reasons[0].Message, response.StatusCode);

    public static IReadOnlyList<SaleDetail> ToDto(IReadOnlyList<Sale> entities) => [.. entities.Select(ToDto)];

    public static Response<IReadOnlyList<SaleDetail>> ToDto(Response<IReadOnlyList<Sale>> response) =>
        response.IsSuccess ?
            ResponseFactory.Ok(ToDto(response.Data)) :
            ResponseFactory.Fail<IReadOnlyList<SaleDetail>>(response.Reasons[0].Message, response.StatusCode);
}
