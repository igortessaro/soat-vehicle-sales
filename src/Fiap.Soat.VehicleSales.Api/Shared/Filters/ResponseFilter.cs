using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Api.Shared.Filters;

public class ResponseFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        if (result is Response<T> { IsSuccess: false } responseTyped)
        {
            return Results.Json(new { Errors = responseTyped.Reasons }, statusCode: (int) responseTyped.StatusCode);
        }

        if (result is Response { IsSuccess: false } response)
        {
            return Results.Json(new { Errors = response.Reasons }, statusCode: (int) response.StatusCode);
        }

        return result;
    }
}
