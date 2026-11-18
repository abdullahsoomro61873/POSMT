
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;
using POSMT.API.Services;

namespace POSMT.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class TerminalsController : ApiControllerBase
{
    private readonly ITerminalService _service;

    public TerminalsController(ITerminalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TerminalDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return ToResponse(await _service.GetByIdAsync(id));
    }

    [Authorize(Roles = RoleNames.Manager)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateTerminalRequest request)
    {
        var result = await _service.CreateAsync(request);

        if (result.Status != ResultStatus.Success)
        {
            return ToResponse(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.TerminalId },
            result.Data);
    }

    [Authorize(Roles = RoleNames.Manager)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateTerminalRequest request)
    {
        return ToResponse(await _service.UpdateAsync(id, request));
    }

    [Authorize(Roles = RoleNames.Manager)]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateTerminalStatusRequest request)
    {
        return ToResponse(
            await _service.UpdateStatusAsync(id, request));
    }
}
