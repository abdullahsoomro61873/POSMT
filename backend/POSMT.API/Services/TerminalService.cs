
using Microsoft.EntityFrameworkCore;
using POSMT.API.Data;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;

namespace POSMT.API.Services;

public class TerminalService : ITerminalService
{
    private readonly AppDbContext _db;

    public TerminalService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TerminalDto>> GetAllAsync()
    {
        return await _db.Terminals
            .AsNoTracking()
            .OrderBy(t => t.TerminalName)
            .Select(t => new TerminalDto(
                t.TerminalId,
                t.TerminalName,
                t.Status))
            .ToListAsync();
    }

    public async Task<ServiceResult<TerminalDto>> GetByIdAsync(int id)
    {
        var terminal = await _db.Terminals
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TerminalId == id);

        if (terminal is null)
        {
            return ServiceResult<TerminalDto>.NotFound(
                "Terminal not found.");
        }

        return ServiceResult<TerminalDto>.Ok(
            new TerminalDto(
                terminal.TerminalId,
                terminal.TerminalName,
                terminal.Status));
    }

    public async Task<ServiceResult<TerminalDto>> CreateAsync(
        CreateTerminalRequest request)
    {
        var name = request.TerminalName.Trim();

        if (await _db.Terminals.AnyAsync(t => t.TerminalName == name))
        {
            return ServiceResult<TerminalDto>.Conflict(
                "A terminal with this name already exists.");
        }

        var terminal = new Terminal
        {
            TerminalName = name,
            Status = "Active"
        };

        _db.Terminals.Add(terminal);
        await _db.SaveChangesAsync();

        return ServiceResult<TerminalDto>.Ok(
            new TerminalDto(
                terminal.TerminalId,
                terminal.TerminalName,
                terminal.Status));
    }

    public async Task<ServiceResult<TerminalDto>> UpdateAsync(
        int id,
        UpdateTerminalRequest request)
    {
        var terminal = await _db.Terminals
            .FirstOrDefaultAsync(t => t.TerminalId == id);

        if (terminal is null)
        {
            return ServiceResult<TerminalDto>.NotFound(
                "Terminal not found.");
        }

        var name = request.TerminalName.Trim();

        if (await _db.Terminals.AnyAsync(
            t => t.TerminalName == name && t.TerminalId != id))
        {
            return ServiceResult<TerminalDto>.Conflict(
                "A terminal with this name already exists.");
        }

        terminal.TerminalName = name;

        await _db.SaveChangesAsync();

        return ServiceResult<TerminalDto>.Ok(
            new TerminalDto(
                terminal.TerminalId,
                terminal.TerminalName,
                terminal.Status));
    }

    public async Task<ServiceResult<TerminalDto>> UpdateStatusAsync(
        int id,
        UpdateTerminalStatusRequest request)
    {
        var terminal = await _db.Terminals
            .FirstOrDefaultAsync(t => t.TerminalId == id);

        if (terminal is null)
        {
            return ServiceResult<TerminalDto>.NotFound(
                "Terminal not found.");
        }

        terminal.Status = request.Status;

        await _db.SaveChangesAsync();

        return ServiceResult<TerminalDto>.Ok(
            new TerminalDto(
                terminal.TerminalId,
                terminal.TerminalName,
                terminal.Status));
    }
}
