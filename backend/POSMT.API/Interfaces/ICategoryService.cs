using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<ServiceResult<CategoryDto>> GetByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryRequest request);
    Task<ServiceResult<CategoryDto>> UpdateAsync(int id, CategoryRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
