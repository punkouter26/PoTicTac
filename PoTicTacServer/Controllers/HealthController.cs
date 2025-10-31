using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PoTicTacServer.Controllers;

/// <summary>
/// API endpoints for system health monitoring and diagnostics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Returns detailed health status of all system dependencies including Azure Table Storage connectivity.
    /// </summary>
    /// <returns>Health check results with status, duration, and individual component health.</returns>
    /// <response code="200">All systems healthy.</response>
    /// <response code="503">One or more systems unhealthy or degraded.</response>
    /// <remarks>
    /// This endpoint checks:
    /// - Azure Table Storage connectivity
    /// - Database read/write operations
    /// - Service availability
    /// 
    /// Use this endpoint for:
    /// - Kubernetes liveness/readiness probes
    /// - Load balancer health checks
    /// - Monitoring and alerting systems
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = healthReport.Status.ToString(),
                totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                checks = healthReport.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration.TotalMilliseconds,
                    exception = entry.Value.Exception?.Message,
                    data = entry.Value.Data
                })
            };

            var statusCode = healthReport.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

            _logger.LogInformation("Health check completed with status: {Status}", healthReport.Status);

            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "Unhealthy",
                error = ex.Message
            });
        }
    }
}
