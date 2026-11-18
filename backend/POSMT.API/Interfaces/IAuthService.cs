using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
}
