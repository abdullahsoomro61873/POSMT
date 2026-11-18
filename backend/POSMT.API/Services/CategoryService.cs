using Microsoft.EntityFrameworkCore;
using POSMT.API.Data;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;

namespace POSMT.API.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryDto(c.CategoryId, c.CategoryName))
            .ToListAsync();
    }

    public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int id)
    {
        var category = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null)
        {
            return ServiceResult<CategoryDto>.NotFound("Category not found.");
        }
        return ServiceResult<CategoryDto>.Ok(new CategoryDto(category.CategoryId, category.CategoryName));
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request)
    {
        var name = request.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c => c.CategoryName == name))
        {
            return ServiceResult<CategoryDto>.Conflict("A category with this name already exists.");
        }

        var category = new Category { CategoryName = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return ServiceResult<CategoryDto>.Ok(new CategoryDto(category.CategoryId, category.CategoryName));
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null)
        {
            return ServiceResult<CategoryDto>.NotFound("Category not found.");
        }

        var name = request.CategoryName.Trim();
        if (await _db.Categories.AnyAsync(c => c.CategoryName == name && c.CategoryId != id))
        {
            return ServiceResult<CategoryDto>.Conflict("A category with this name already exists.");
        }

        category.CategoryName = name;
        await _db.SaveChangesAsync();
        return ServiceResult<CategoryDto>.Ok(new CategoryDto(category.CategoryId, category.CategoryName));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null)
        {
            return ServiceResult<bool>.NotFound("Category not found.");
        }

        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            return ServiceResult<bool>.Conflict("This category has products and cannot be deleted.");
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
