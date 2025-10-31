using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace PoTicTacServer.Telemetry;

/// <summary>
/// Serilog enricher that adds correlation properties to all log messages
/// </summary>
public class CorrelationEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Add CorrelationId from current Activity (OpenTelemetry)
        var activity = Activity.Current;
        if (activity != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", activity.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ParentId", activity.ParentId ?? ""));
        }

        if (httpContext != null)
        {
            // Add UserId if authenticated
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirst("sub")?.Value
                    ?? httpContext.User.FindFirst("userId")?.Value
                    ?? httpContext.User.Identity.Name
                    ?? "anonymous";

                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            }
            else
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", "anonymous"));
            }

            // Add TenantId if applicable (for multi-tenant scenarios)
            var tenantId = httpContext.User?.FindFirst("tenantId")?.Value ?? "default";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TenantId", tenantId));

            // Add request path
            if (httpContext.Request?.Path.HasValue == true)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", httpContext.Request.Path.Value));
            }

            // Add client IP
            var clientIp = httpContext.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ClientIp", clientIp));

            // Add session ID if available
            if (httpContext.Session != null && httpContext.Session.IsAvailable)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SessionId", httpContext.Session.Id));
            }
        }
    }
}
