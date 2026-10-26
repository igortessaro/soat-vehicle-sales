using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Domain.Shared;

[ExcludeFromCodeCoverage]
public class Paginate<T> : IPaginate<T>
{
    public Paginate(IEnumerable<T> items, int totalCount, int pageSize, int currentPage, int totalPages)
    {
        Items = items;
        TotalCount = totalCount;
        PageSize = pageSize;
        CurrentPage = currentPage;
        TotalPages = totalPages;
    }

    public IEnumerable<T> Items { get; private set; }
    public int TotalCount { get; private set; }
    public int PageSize { get; private set; }
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
}
