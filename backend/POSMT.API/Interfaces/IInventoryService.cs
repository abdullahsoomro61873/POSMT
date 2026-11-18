using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<InventoryDto>> SearchAsync(InventoryQuery query);
    Task<ServiceResult<InventoryDto>> GetByProductIdAsync(int productId);
    Task<ServiceResult<InventoryDto>> AdjustStockAsync(int productId, StockAdjustRequest request);
    Task<ServiceResult<InventoryDto>> SetReorderLevelAsync(int productId, ReorderLevelRequest request);
}
