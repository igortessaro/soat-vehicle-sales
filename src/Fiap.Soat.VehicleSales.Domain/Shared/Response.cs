using FluentResults;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace Fiap.Soat.VehicleSales.Domain.Shared;

[ExcludeFromCodeCoverage]
public sealed class Response<T>(Result<T> result, HttpStatusCode statusCode)
{
    [JsonIgnore]
    public Result<T> InnerResult { get; } = result;
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; } = statusCode;

    public bool IsSuccess => InnerResult.IsSuccess;
    public T Data => InnerResult.ValueOrDefault;
    public List<IReason> Reasons => InnerResult.Reasons;

    public static Response<T> Ok(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(Result.Ok(value), statusCode);

    public static Response<T> Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(Result.Fail<T>(message), statusCode);

    public static Response<T> Fail(Error error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail<T>(error), statusCode);

    public static Response<T> Fail(List<IError> errors, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail<T>(errors), statusCode);

    public static implicit operator Result<T>(Response<T> result)
        => result.InnerResult;

    public static implicit operator Response(Response<T> result)
        => result.IsSuccess ?
            ResponseFactory.Ok(result.StatusCode) :
            ResponseFactory.Fail(result.Reasons.Select(x => x.Message).ToArray(), result.StatusCode);

}

public sealed class Response(Result result, HttpStatusCode statusCode)
{
    [JsonIgnore]
    public Result InnerResult { get; } = result;
    [JsonIgnore]
    public HttpStatusCode StatusCode { get; } = statusCode;

    public bool IsSuccess => InnerResult.IsSuccess;
    public List<IReason> Reasons => InnerResult.Reasons;

    public static Response<object> Ok(object value, HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(Result.Ok(value), statusCode);

    public static Response Ok(HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(Result.Ok(), statusCode);

    public static Response Fail(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(Result.Fail(message), statusCode);

    public static Response Fail(Error error, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(Result.Fail(error), statusCode);

    public static Response Fail(List<IError> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(Result.Fail(errors), statusCode);

    public static implicit operator Result(Response result)
        => result.InnerResult;
}

public static class ResponseFactory
{
    public static Response Ok(HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(Result.Ok(), statusCode);

    public static Response<T> Ok<T>(T value, HttpStatusCode statusCode = HttpStatusCode.OK)
        => new(Result.Ok(value), statusCode);

    public static Response<T> Fail<T>(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        => new(Result.Fail<T>(message), statusCode);

    public static Response<T> Fail<T>(Error error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail<T>(error), statusCode);

    public static Response Fail(Error error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail(error), statusCode);

    public static Response Fail(string error, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail(error), statusCode);

    public static Response Fail(string[] errors, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail(errors), statusCode);

    public static Response<T> Fail<T>(List<IError> errors, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        => new(Result.Fail<T>(errors), statusCode);
}
