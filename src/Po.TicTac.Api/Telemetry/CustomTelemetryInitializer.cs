using System.Diagnostics;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Po.TicTac.Api.Telemetry;

/// <summary>
/// Application Insights telemetry initializer that enriches all telemetry with custom properties
/// </summary>
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Add correlation properties from current Activity
        var activity = Activity.Current;
        if (activity != null)
        {
            telemetry.Context.Operation.Id = activity.TraceId.ToString();
            telemetry.Context.Operation.ParentId = activity.ParentId;
        }

        if (httpContext != null)
        {
            // Add UserId
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value
                    ?? httpContext.User.Identity.Name
                    ?? "anonymous";

                telemetry.Context.User.Id = userId;
                telemetry.Context.User.AuthenticatedUserId = userId;
            }
            else
            {
                telemetry.Context.User.Id = "anonymous";
            }

            // Add TenantId as custom property
            var tenantId = httpContext.User?.FindFirst("tenantId")?.Value ?? "default";
            if (!telemetry.Context.GlobalProperties.ContainsKey("TenantId"))
            {
                telemetry.Context.GlobalProperties.Add("TenantId", tenantId);
            }

            // Add UserRole if available
            var userRole = httpContext.User?.FindFirst("role")?.Value ?? "guest";
            if (!telemetry.Context.GlobalProperties.ContainsKey("UserRole"))
            {
                telemetry.Context.GlobalProperties.Add("UserRole", userRole);
            }

            // Add GameMode from request headers or query (if applicable)
            if (httpContext.Request.Query.TryGetValue("gameMode", out var gameMode))
            {
                if (!telemetry.Context.GlobalProperties.ContainsKey("GameMode"))
                {
                    telemetry.Context.GlobalProperties.Add("GameMode", gameMode.ToString());
                }
            }

            // Add Difficulty from request headers or query (if applicable)
            if (httpContext.Request.Query.TryGetValue("difficulty", out var difficulty))
            {
                if (!telemetry.Context.GlobalProperties.ContainsKey("Difficulty"))
                {
                    telemetry.Context.GlobalProperties.Add("Difficulty", difficulty.ToString());
                }
            }

            // Add environment information
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            if (!telemetry.Context.GlobalProperties.ContainsKey("Environment"))
            {
                telemetry.Context.GlobalProperties.Add("Environment", environment);
            }

            // Add application version
            var version = typeof(CustomTelemetryInitializer).Assembly.GetName().Version?.ToString() ?? "unknown";
            if (!telemetry.Context.GlobalProperties.ContainsKey("AppVersion"))
            {
                telemetry.Context.GlobalProperties.Add("AppVersion", version);
            }
        }
    }
}
