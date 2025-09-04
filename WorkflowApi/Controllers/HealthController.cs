using Microsoft.AspNetCore.Mvc;

namespace WorkflowApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API is healthy");
    }
}