using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query);
    Task<ServiceResult<ProductDto>> GetByIdAsync(int id);
    Task<ServiceResult<ProductDto>> CreateAsync(ProductCreateRequest request);
    Task<ServiceResult<ProductDto>> UpdateAsync(int id, ProductUpdateRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
