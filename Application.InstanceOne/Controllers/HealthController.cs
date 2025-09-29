using System.Net.Mime;
using Application.Services;
using Domain.Controllers.Abstractions;
using Microsoft.AspNetCore.Mvc;
using TRECs.Library.Domain;

namespace Application.InstanceOne.Controllers;

public class HealthController : PublicApiBaseController<HealthController>
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet("")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(PublicResponse<dynamic>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthCheckAsync()
    {
        var response = await _healthCheckService.HealthCheckAsync();
        return ProcessResponsePublicMessage(response);
    }
}