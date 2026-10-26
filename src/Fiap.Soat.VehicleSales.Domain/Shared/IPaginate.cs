using System.Collections.Generic;

namespace Fiap.Soat.VehicleSales.Domain.Shared;

public interface IPaginate<out T>
{
    IEnumerable<T> Items { get; }
    int TotalCount { get; }
    int PageSize { get; }
    int CurrentPage { get; }
    int TotalPages { get; }
}
