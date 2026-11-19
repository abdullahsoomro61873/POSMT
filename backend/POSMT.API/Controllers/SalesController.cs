using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Models;
using POSMT.API.Services;

namespace POSMT.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class SalesController : ApiControllerBase
{
    private readonly ISaleService _service;

    public SalesController(ISaleService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaleCreateRequest request)
    {
        var cashierId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _service.CreateAsync(cashierId, request);

        if (result.Status != ResultStatus.Success)
        {
            return ToResponse(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.SaleId },
            result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var isManager = User.IsInRole(RoleNames.Manager);

        return ToResponse(
            await _service.GetByIdAsync(id, userId, isManager));
    }
}
