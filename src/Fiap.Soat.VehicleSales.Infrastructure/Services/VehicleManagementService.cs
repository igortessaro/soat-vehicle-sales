using Fiap.Soat.VehicleSales.Application.Adapters.Gateways.Services;
using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Fiap.Soat.VehicleSales.Infrastructure.Services;

public sealed class VehicleManagementService(
    ILogger<VehicleManagementService> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IVehicleManagementService
{
    private const string VehicleManagementBaseUrlConfigKey = "VehicleManagement:BaseUrl";

    public async Task<Response<VehicleDetail>> ReserveVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        var response = await httpClient.PostAsync($"/api/v1/internal/vehicles/{vehicleId}/reserve", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to reserve vehicle {VehicleId}. Status code: {StatusCode}", vehicleId, response.StatusCode);
            return ResponseFactory.Fail<VehicleDetail>($"Failed to reserve vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDetail>>(cancellationToken);
        if (result == null) return ResponseFactory.Fail<VehicleDetail>($"Vehicle {vehicleId} not found");

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to reserve vehicle {VehicleId}. Reasons: {Reasons}", vehicleId, string.Join(", ", result.Reasons));
            return ResponseFactory.Fail<VehicleDetail>(string.Join(", ", result.Reasons));
        }

        return ResponseFactory.Ok(result.Data);
    }

    public async Task<Response<VehicleDetail>> ReleaseVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        var response = await httpClient.PostAsync($"/api/v1/internal/vehicles/{vehicleId}/release", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to release vehicle {VehicleId}. Status code: {StatusCode}", vehicleId, response.StatusCode);
            return ResponseFactory.Fail<VehicleDetail>($"Failed to release vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDetail>>(cancellationToken);
        if (result == null) return ResponseFactory.Fail<VehicleDetail>($"Vehicle {vehicleId} not found");

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to release vehicle {VehicleId}. Reasons: {Reasons}", vehicleId, string.Join(", ", result.Reasons));
            return ResponseFactory.Fail<VehicleDetail>(string.Join(", ", result.Reasons));
        }

        return ResponseFactory.Ok(result.Data);
    }

    public async Task<Response<VehicleDetail>> SellVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        var response = await httpClient.PostAsync($"/api/v1/internal/vehicles/{vehicleId}/sell", null, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to sell vehicle {VehicleId}. Status code: {StatusCode}", vehicleId, response.StatusCode);
            return ResponseFactory.Fail<VehicleDetail>($"Failed to sell vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDetail>>(cancellationToken);
        if (result == null) return ResponseFactory.Fail<VehicleDetail>($"Vehicle {vehicleId} not found");

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to sell vehicle {VehicleId}. Reasons: {Reasons}", vehicleId, string.Join(", ", result.Reasons));
            return ResponseFactory.Fail<VehicleDetail>(string.Join(", ", result.Reasons));
        }

        return ResponseFactory.Ok(result.Data);
    }

    public async Task<Response<VehicleDetail>> GetAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        var response = await httpClient.GetAsync($"/api/v1/vehicles/{vehicleId}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get vehicle {VehicleId}. Status code: {StatusCode}", vehicleId, response.StatusCode);
            return ResponseFactory.Fail<VehicleDetail>($"Failed to get vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<VehicleDetail>>(cancellationToken);
        if (result == null) return ResponseFactory.Fail<VehicleDetail>($"Vehicle {vehicleId} not found");

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to get vehicle {VehicleId}. Reasons: {Reasons}", vehicleId, string.Join(", ", result.Reasons));
            return ResponseFactory.Fail<VehicleDetail>(string.Join(", ", result.Reasons));
        }

        return ResponseFactory.Ok(result.Data);
    }

    public Task<Response<IReadOnlyList<VehicleDetail>>> GetAllAvailableAsync(CancellationToken cancellationToken = default) =>
        GetAllVehiclesAsync("available", cancellationToken);

    public Task<Response<IReadOnlyList<VehicleDetail>>> GetAllSoldAsync(CancellationToken cancellationToken = default) =>
        GetAllVehiclesAsync("sold", cancellationToken);

    private async Task<Response<IReadOnlyList<VehicleDetail>>> GetAllVehiclesAsync(string status, CancellationToken cancellationToken = default)
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(configuration[VehicleManagementBaseUrlConfigKey] ?? throw new InvalidOperationException("VehicleManagement:BaseUrl not configured"));
        var response = await httpClient.GetAsync($"/api/v1/vehicles/{status}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get all {Status} vehicles. Status code: {StatusCode}", status, response.StatusCode);
            return ResponseFactory.Fail<IReadOnlyList<VehicleDetail>>($"Failed to get vehicle. Error: {response.ReasonPhrase}");
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VehicleDetail>>>(cancellationToken);
        return result == null ?
            ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>(new List<VehicleDetail>()) :
            ResponseFactory.Ok<IReadOnlyList<VehicleDetail>>(result.Data);
    }
}

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public List<string> Reasons { get; set; } = new();
    public bool IsSuccess { get; set; }
}
