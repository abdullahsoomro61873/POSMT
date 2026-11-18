using Microsoft.AspNetCore.Mvc;
using POSMT.API.DTOs;
using POSMT.API.Interfaces;
using POSMT.API.Services;

namespace POSMT.API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return ToResponse(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (result.Status != ResultStatus.Success)
        {
            return ToResponse(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.CategoryId }, result.Data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
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
