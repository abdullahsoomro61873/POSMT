using Microsoft.AspNetCore.Mvc;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Services;

namespace POSMT.API.Controllers;

[Route("api/[controller]")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Search([FromQuery] ProductQuery query)
    {
        return Ok(await _service.SearchAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return ToResponse(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (result.Status != ResultStatus.Success)
        {
            return ToResponse(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.ProductId }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateRequest request)
    {
        return ToResponse(await _service.UpdateAsync(id, request));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Status == ResultStatus.Success ? NoContent() : ToResponse(result);
    }
}
