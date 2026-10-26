using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Api.Shared.BackgroundServices;

[ExcludeFromCodeCoverage]
public sealed class VehicleSyncOptions
{
    public const string SectionName = "VehicleSync";

    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 10;
}
