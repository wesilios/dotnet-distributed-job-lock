using System.Net.Mime;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using TRECs.Library.Domain;

namespace Application.InstanceTwo.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : Controller
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PublicResponse<dynamic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthCheckAsync()
    {
        var response = await _healthCheckService.HealthCheckAsync();
        return Ok(response);
    }
}