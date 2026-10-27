using Fiap.Soat.VehicleSales.Application.Models;
using Fiap.Soat.VehicleSales.Domain.ValueObjects;
using System;

namespace Fiap.Soat.VehicleSales.Application.Mappers;

public static class VehicleMapper
{
    public static VehicleSnapshot ToSnapshot(this VehicleDetail vehicle) =>
        new(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.LicensePlate,
            vehicle.Year,
            vehicle.Color,
            vehicle.Price,
            Enum.Parse<VehicleStatus>(vehicle.Status, ignoreCase: true),
            vehicle.CreatedAt,
            vehicle.UpdatedAt);

    public static VehicleDetail ToDetail(this Domain.Entities.Vehicle vehicle) =>
        new(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.LicensePlate,
            vehicle.ManufactureYear,
            vehicle.Color,
            vehicle.Price,
            vehicle.Status.ToString(),
            vehicle.SourceCreatedAt,
            vehicle.SourceUpdatedAt);
}
