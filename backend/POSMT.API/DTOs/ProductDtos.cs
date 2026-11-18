using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

public record ProductDto(
    int ProductId,
    string Name,
    string? Barcode,
    decimal Price,
    int CategoryId,
    string CategoryName,
    bool IsActive,
    int Quantity,
    int ReorderLevel);

public class ProductCreateRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Barcode { get; set; }

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, 1000000)]
    public int InitialStock { get; set; }

    [Range(0, 1000000)]
    public int ReorderLevel { get; set; }
}

public class ProductUpdateRequest
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Barcode { get; set; }

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProductQuery
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool IncludeInactive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);
