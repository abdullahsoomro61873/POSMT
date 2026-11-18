using Microsoft.AspNetCore.Mvc;
using POSMT.API.Services;

namespace POSMT.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToResponse<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Success => Ok(result.Data),
            ResultStatus.NotFound => NotFound(new ProblemDetails { Title = result.Error, Status = 404 }),
            ResultStatus.Conflict => Conflict(new ProblemDetails { Title = result.Error, Status = 409 }),
            ResultStatus.Unauthorized => Unauthorized(new ProblemDetails { Title = result.Error, Status = 401 }),
            _ => BadRequest(new ProblemDetails { Title = result.Error, Status = 400 })
        };
    }
}
