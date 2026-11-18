using System.ComponentModel.DataAnnotations;

namespace POSMT.API.DTOs;

public record InventoryDto(
    int ProductId,
    string ProductName,
    string? Barcode,
    int Quantity,
    int ReorderLevel,
    bool IsLowStock);

public class StockAdjustRequest
{
    [Range(-1000000, 1000000)]
    public int QuantityChange { get; set; }
}

public class ReorderLevelRequest
{
    [Range(0, 1000000)]
    public int ReorderLevel { get; set; }
}

public class InventoryQuery
{
    public string? Search { get; set; }
    public bool LowStockOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
