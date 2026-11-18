using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using POSMT.API.Data;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;

namespace POSMT.API.Services;

public class InventoryService : IInventoryService
{
    private const int MaxRetries = 5;

    private static readonly Expression<Func<Inventory, InventoryDto>> ToDto = i => new InventoryDto(
        i.ProductId,
        i.Product.Name,
        i.Product.Barcode,
        i.Quantity,
        i.ReorderLevel,
        i.Quantity <= i.ReorderLevel);

    private readonly AppDbContext _db;

    public InventoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<InventoryDto>> SearchAsync(InventoryQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var inventories = _db.Inventories.AsNoTracking().Where(i => i.Product.IsActive);

        if (query.LowStockOnly)
        {
            inventories = inventories.Where(i => i.Quantity <= i.ReorderLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            inventories = inventories.Where(i =>
                i.Product.Name.Contains(term) || (i.Product.Barcode != null && i.Product.Barcode.Contains(term)));
        }

        var totalCount = await inventories.CountAsync();
        var items = await inventories
            .OrderBy(i => i.Product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync();

        return new PagedResult<InventoryDto>(items, page, pageSize, totalCount);
    }

    public async Task<ServiceResult<InventoryDto>> GetByProductIdAsync(int productId)
    {
        var dto = await _db.Inventories
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        return dto is null
            ? ServiceResult<InventoryDto>.NotFound("Inventory record not found.")
            : ServiceResult<InventoryDto>.Ok(dto);
    }

    public async Task<ServiceResult<InventoryDto>> AdjustStockAsync(int productId, StockAdjustRequest request)
    {
        if (request.QuantityChange == 0)
        {
            return ServiceResult<InventoryDto>.Invalid("Quantity change cannot be zero.");
        }

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
            if (inventory is null)
            {
                return ServiceResult<InventoryDto>.NotFound("Inventory record not found.");
            }

            var newQuantity = inventory.Quantity + request.QuantityChange;
            if (newQuantity < 0)
            {
                return ServiceResult<InventoryDto>.Conflict($"Insufficient stock. Available: {inventory.Quantity}.");
            }

            inventory.Quantity = newQuantity;

            try
            {
                await _db.SaveChangesAsync();
                return await GetByProductIdAsync(productId);
            }
            catch (DbUpdateConcurrencyException)
            {
                _db.ChangeTracker.Clear();
            }
        }

        return ServiceResult<InventoryDto>.Conflict("Stock was changed by another terminal. Please try again.");
    }

    public async Task<ServiceResult<InventoryDto>> SetReorderLevelAsync(int productId, ReorderLevelRequest request)
    {
        var inventory = await _db.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (inventory is null)
        {
            return ServiceResult<InventoryDto>.NotFound("Inventory record not found.");
        }

        inventory.ReorderLevel = request.ReorderLevel;
        await _db.SaveChangesAsync();
        return await GetByProductIdAsync(productId);
    }
}
