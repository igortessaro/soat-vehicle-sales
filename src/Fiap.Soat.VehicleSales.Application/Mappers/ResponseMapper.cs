using Fiap.Soat.VehicleSales.Domain.Shared;
using System;
using System.Linq;

namespace Fiap.Soat.VehicleSales.Application.Mappers;

public static class ResponseMapper
{
    public static Response<TOut> Map<TIn, TOut>(Response<TIn> response, Func<TIn, TOut> converter)
    {
        if (!response.IsSuccess)
        {
            string error = response.Reasons.Select(x => x.Message).FirstOrDefault() ?? string.Empty;
            return ResponseFactory.Fail<TOut>(error, response.StatusCode);
        }

        return ResponseFactory.Ok(converter(response.Data), response.StatusCode);
    }

    public static Response<Paginate<TOut>> Map<TIn, TOut>(Response<Paginate<TIn>> response, Func<TIn, TOut> converter)
    {
        if (!response.IsSuccess)
        {
            string error = response.Reasons.Select(x => x.Message).FirstOrDefault() ?? string.Empty;
            return ResponseFactory.Fail<Paginate<TOut>>(error, response.StatusCode);
        }

        var dtoPaginate = new Paginate<TOut>(
            response.Data.Items.Select(converter).ToList(),
            response.Data.TotalCount,
            response.Data.PageSize,
            response.Data.CurrentPage,
            response.Data.TotalPages);

        return ResponseFactory.Ok(dtoPaginate, response.StatusCode);
    }
}
