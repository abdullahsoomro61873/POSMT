using POSMT.API.DTOs;
using POSMT.API.Services;

namespace POSMT.API.Interfaces;

public interface ITerminalService
{
    Task<List<TerminalDto>> GetAllAsync();

    Task<ServiceResult<TerminalDto>> GetByIdAsync(int id);

    Task<ServiceResult<TerminalDto>> CreateAsync(
        CreateTerminalRequest request);

    Task<ServiceResult<TerminalDto>> UpdateAsync(
        int id,
        UpdateTerminalRequest request);

    Task<ServiceResult<TerminalDto>> UpdateStatusAsync(
        int id,
        UpdateTerminalStatusRequest request);
}