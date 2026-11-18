using Microsoft.AspNetCore.Mvc;
using POSMT.API.Data;

namespace POSMT.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;

    public HealthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var databaseConnected = await _db.Database.CanConnectAsync();
        return Ok(new
        {
            status = "Healthy",
            database = databaseConnected ? "Connected" : "Unavailable",
            timeUtc = DateTime.UtcNow
        });
    }
}
