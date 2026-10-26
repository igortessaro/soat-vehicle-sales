using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Integration.Tests;

/// <summary>
/// Replaces the primary handler of the default HttpClient, standing in for vehicle-management and payment-gateway.
/// Unmatched requests return 501 so a missing setup fails loudly instead of hanging on the network.
/// </summary>
public sealed class HttpStub : HttpMessageHandler
{
    private readonly ConcurrentDictionary<string, Func<HttpResponseMessage>> _routes = new();
    private readonly ConcurrentQueue<RecordedRequest> _requests = new();

    public IReadOnlyCollection<RecordedRequest> Requests => _requests;

    public void Setup(HttpMethod method, string url, HttpStatusCode statusCode, object? body = null) =>
        _routes[Key(method, new Uri(url))] = () => new HttpResponseMessage(statusCode)
        {
            Content = body is null ? null : JsonContent.Create(body)
        };

    public void Reset()
    {
        _routes.Clear();
        _requests.Clear();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        _requests.Enqueue(new RecordedRequest(request.Method, request.RequestUri!, body));

        return _routes.TryGetValue(Key(request.Method, request.RequestUri!), out var response)
            ? response()
            : new HttpResponseMessage(HttpStatusCode.NotImplemented);
    }

    private static string Key(HttpMethod method, Uri uri) => $"{method} {uri.GetLeftPart(UriPartial.Path)}";
}

public sealed record RecordedRequest(HttpMethod Method, Uri Uri, string? Body);
