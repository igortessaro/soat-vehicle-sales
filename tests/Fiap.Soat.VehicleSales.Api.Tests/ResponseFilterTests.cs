using Fiap.Soat.VehicleSales.Api.Shared.Filters;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Fiap.Soat.VehicleSales.Api.Tests;

public class ResponseFilterTests
{
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.Conflict)]
    public async Task InvokeAsync_WhenTypedResponseFails_ReturnsJsonWithResponseStatusCode(HttpStatusCode statusCode)
    {
        var filter = new ResponseFilter<string>();
        var context = new TestInvocationContext();

        var result = await filter.InvokeAsync(context, _ =>
            ValueTask.FromResult<object?>(ResponseFactory.Fail<string>("invalid", statusCode)));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<IStatusCodeHttpResult>());
            Assert.That(((IStatusCodeHttpResult) result!).StatusCode, Is.EqualTo((int) statusCode));
        });
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task InvokeAsync_WhenResponseFails_ReturnsJsonWithResponseStatusCode(HttpStatusCode statusCode)
    {
        var filter = new ResponseFilter<string>();
        var context = new TestInvocationContext();

        var result = await filter.InvokeAsync(context, _ =>
            ValueTask.FromResult<object?>(ResponseFactory.Fail("invalid", statusCode)));

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<IStatusCodeHttpResult>());
            Assert.That(((IStatusCodeHttpResult) result!).StatusCode, Is.EqualTo((int) statusCode));
        });
    }

    [Test]
    public async Task InvokeAsync_WhenTypedResponseSucceeds_ReturnsOriginalResult()
    {
        var filter = new ResponseFilter<string>();
        var expected = ResponseFactory.Ok("ok");
        var context = new TestInvocationContext();

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(expected));

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public async Task InvokeAsync_WhenResponseSucceeds_ReturnsOriginalResult()
    {
        var filter = new ResponseFilter<string>();
        var expected = ResponseFactory.Ok();
        var context = new TestInvocationContext();

        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(expected));

        Assert.That(result, Is.SameAs(expected));
    }

    private sealed class TestInvocationContext : EndpointFilterInvocationContext
    {
        public override HttpContext HttpContext { get; } = new DefaultHttpContext();
        public override IList<object?> Arguments { get; } = [];

        public override T GetArgument<T>(int index) => (T) Arguments[index]!;
    }
}
