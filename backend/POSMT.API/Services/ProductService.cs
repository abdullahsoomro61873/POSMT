using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using POSMT.API.Data;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;

namespace POSMT.API.Services;

public class ProductService : IProductService
{
    private static readonly Expression<Func<Product, ProductDto>> ToDto = p => new ProductDto(
        p.ProductId,
        p.Name,
        p.Barcode,
        p.Price,
        p.CategoryId,
        p.Category.CategoryName,
        p.IsActive,
        p.Inventory != null ? p.Inventory.Quantity : 0,
        p.Inventory != null ? p.Inventory.ReorderLevel : 0);

    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var products = _db.Products.AsNoTracking().AsQueryable();

        if (!query.IncludeInactive)
        {
            products = products.Where(p => p.IsActive);
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => p.Name.Contains(term) || (p.Barcode != null && p.Barcode.Contains(term)));
        }

        var totalCount = await products.CountAsync();
        var items = await products
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToListAsync();

        return new PagedResult<ProductDto>(items, page, pageSize, totalCount);
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(int id)
    {
        var dto = await _db.Products
            .AsNoTracking()
            .Where(p => p.ProductId == id)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        return dto is null
            ? ServiceResult<ProductDto>.NotFound("Product not found.")
            : ServiceResult<ProductDto>.Ok(dto);
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(ProductCreateRequest request)
    {
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId))
        {
            return ServiceResult<ProductDto>.Invalid("Category does not exist.");
        }

        var barcode = NormalizeBarcode(request.Barcode);
        if (barcode != null && await _db.Products.AnyAsync(p => p.Barcode == barcode))
        {
            return ServiceResult<ProductDto>.Conflict("A product with this barcode already exists.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Barcode = barcode,
            Price = request.Price,
            CategoryId = request.CategoryId,
            Inventory = new Inventory
            {
                Quantity = request.InitialStock,
                ReorderLevel = request.ReorderLevel
            }
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(product.ProductId);
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(int id, ProductUpdateRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null)
        {
            return ServiceResult<ProductDto>.NotFound("Product not found.");
        }

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId))
        {
            return ServiceResult<ProductDto>.Invalid("Category does not exist.");
        }

        var barcode = NormalizeBarcode(request.Barcode);
        if (barcode != null && await _db.Products.AnyAsync(p => p.Barcode == barcode && p.ProductId != id))
        {
            return ServiceResult<ProductDto>.Conflict("A product with this barcode already exists.");
        }

        product.Name = request.Name.Trim();
        product.Barcode = barcode;
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        if (product is null)
        {
            return ServiceResult<bool>.NotFound("Product not found.");
        }

        if (await _db.SaleItems.AnyAsync(s => s.ProductId == id))
        {
            product.IsActive = false;
        }
        else
        {
            _db.Products.Remove(product);
        }

        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }

    private static string? NormalizeBarcode(string? barcode)
    {
        return string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
    }
}
