using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Fiap.Soat.VehicleSales.Infrastructure.Services;

public sealed class VehicleManagementService(
    ILogger<VehicleManagementService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IVehicleManagementService
{
    private const string VehicleManagementBaseUrlConfigKey = "VehicleManagement:BaseUrl";

    public Task<Response<VehicleDetail>> ReserveVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        SendVehicleRequestAsync(vehicleId, "reserve", HttpMethod.Post, $"/api/v1/internal/vehicles/{vehicleId}/reserve", cancellationToken);

    public Task<Response<VehicleDetail>> ReleaseVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        SendVehicleRequestAsync(vehicleId, "release", HttpMethod.Post, $"/api/v1/internal/vehicles/{vehicleId}/release", cancellationToken);

    public Task<Response<VehicleDetail>> SellVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        SendVehicleRequestAsync(vehicleId, "sell", HttpMethod.Post, $"/api/v1/internal/vehicles/{vehicleId}/sell", cancellationToken);

    public Task<Response<VehicleDetail>> DeliverVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        SendVehicleRequestAsync(vehicleId, "deliver", HttpMethod.Post, $"/api/v1/internal/vehicles/{vehicleId}/deliver", cancellationToken);

    public Task<Response<VehicleDetail>> GetAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        SendVehicleRequestAsync(vehicleId, "get", HttpMethod.Get, $"/api/v1/vehicles/{vehicleId}", cancellationToken);

    public Task<Response<IReadOnlyList<VehicleDetail>>> GetAllAvailableAsync(CancellationToken cancellationToken = default) =>
        GetAllVehiclesAsync("/api/v1/vehicles/available", cancellationToken);

    public Task<Response<IReadOnlyList<VehicleDetail>>> GetAllSoldAsync(CancellationToken cancellationToken = default) =>
        GetAllVehiclesAsync("/api/v1/vehicles/sold", cancellationToken);

    public Task<Response<IReadOnlyList<VehicleDetail>>> GetUpdatedSinceAsync(DateTime? updatedSince, CancellationToken cancellationToken = default)
    {
        if (updatedSince is null) return GetAllVehiclesAsync("/api/v1/vehicles", cancellationToken);
        var since = DateTime.SpecifyKind(updatedSince.Value, DateTimeKind.Utc).ToString("O", CultureInfo.InvariantCulture);
        return GetAllVehiclesAsync($"/api/v1/vehicles?updatedSince={Uri.EscapeDataString(since)}", cancellationToken);
    }

    private async Task<Response<VehicleDetail>> SendVehicleRequestAsync(
        Guid vehicleId, string operation, HttpMethod method, string path, CancellationToken cancellationToken)
    {
        using var httpClient = CreateClient();
        using var request = new HttpRequestMessage(method, path);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to {Operation} vehicle {VehicleId}. Status code: {StatusCode}", operation, vehicleId, response.StatusCode);
            return ResponseFactory.Fail<VehicleDetail>($"Failed to {operation} vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDetail>>(cancellationToken);
        if (result == null) return ResponseFactory.Fail<VehicleDetail>($"Vehicle {vehicleId} not found");

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to {Operation} vehicle {VehicleId}. Reasons: {Reasons}", operation, vehicleId, string.Join(", ", result.Reasons));
            return ResponseFactory.Fail<VehicleDetail>(string.Join(", ", result.Reasons));
        }

        return ResponseFactory.Ok(result.Data);
    }

    private async Task<Response<IReadOnlyList<VehicleDetail>>> GetAllVehiclesAsync(string path, CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateClient();
        using var response = await httpClient.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get vehicles from {Path}. Status code: {StatusCode}", path, response.StatusCode);
            return ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>($"Failed to get vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VehicleDetail>>>(cancellationToken);
        return result == null ?
            ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>(new List<VehicleDetail>()) :
            ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>(result.Data);
    }

    private HttpClient CreateClient()
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        return httpClient;
    }
}

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public List<string> Reasons { get; set; } = new();
    public bool IsSuccess { get; set; }
}
