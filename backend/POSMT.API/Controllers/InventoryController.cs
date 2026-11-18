using Microsoft.AspNetCore.Mvc;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;

namespace POSMT.API.Controllers;

[Route("api/[controller]")]
public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InventoryDto>>> Search([FromQuery] InventoryQuery query)
    {
        return Ok(await _service.SearchAsync(query));
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetByProductId(int productId)
    {
        return ToResponse(await _service.GetByProductIdAsync(productId));
    }

    [HttpPost("{productId:int}/adjust")]
    public async Task<IActionResult> Adjust(int productId, StockAdjustRequest request)
    {
        return ToResponse(await _service.AdjustStockAsync(productId, request));
    }

    [HttpPut("{productId:int}/reorder-level")]
    public async Task<IActionResult> SetReorderLevel(int productId, ReorderLevelRequest request)
    {
        return ToResponse(await _service.SetReorderLevelAsync(productId, request));
    }
}
