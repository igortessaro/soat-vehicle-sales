using Fiap.Soat.VehicleSales.Application.Models;
using MediatR;

namespace Fiap.Soat.VehicleSales.Application.UseCases.Vehicle.Copy;

public record VehicleChangedNotification(VehicleDetail Vehicle) : INotification;
