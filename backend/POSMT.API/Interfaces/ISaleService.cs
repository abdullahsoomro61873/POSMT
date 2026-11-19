using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface ISaleService
{
    Task<ServiceResult<SaleDto>> CreateAsync(int cashierId, SaleCreateRequest request);
    Task<ServiceResult<SaleDto>> GetByIdAsync(int id, int userId, bool isManager);
}
