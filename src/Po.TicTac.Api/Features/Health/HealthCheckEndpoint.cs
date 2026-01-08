using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Po.TicTac.Api.Features.Health;

/// <summary>
/// Endpoint configuration for Health Check
/// </summary>
public static class HealthCheckEndpoint
{
    public static IEndpointRouteBuilder MapHealthCheck(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };

            return report.Status == HealthStatus.Healthy
                ? Results.Ok(response)
                : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Health check endpoint")
        .WithDescription("Validates connectivity to all external dependencies including Azure Table Storage.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
