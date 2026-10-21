using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.VehicleSales.Domain.Shared;

[ExcludeFromCodeCoverage]
public class PaginatedRequest
{
    public PaginatedRequest() { }

    public PaginatedRequest(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    [Required]
    [DefaultValue(1)]
    public int PageNumber { get; set; } = 1;
    [Required]
    [DefaultValue(15)]
    public int PageSize { get; set; } = 15;
}
