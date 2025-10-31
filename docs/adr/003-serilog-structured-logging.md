# ADR-003: Use Serilog for Structured Logging

## Status
**Accepted** - August 2, 2025

## Context

We needed a comprehensive logging solution for PoTicTac that would provide:

- **Structured Logging**: Log events with structured data (not just text strings)
- **Cloud Telemetry**: Integration with Azure Application Insights for production monitoring
- **Local Development**: Rich console output for debugging during development
- **Performance**: Minimal performance overhead
- **Correlation**: Ability to trace requests across distributed systems
- **Enrichment**: Automatic addition of contextual information (thread ID, process ID, request path)
- **Flexibility**: Easy configuration for different environments (development vs. production)
- **ASP.NET Core Integration**: Seamless integration with ASP.NET Core logging infrastructure

Traditional logging frameworks like `ILogger` with string-based messages make it difficult to query and analyze logs in production. We needed semantic logging where each log event has structured properties that can be queried.

## Decision

We will use **Serilog** as the primary logging framework with **Application Insights sink** for production telemetry.

### Configuration

**Development Environment**:
```csharp
.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties:j}{NewLine}{Exception}")
.WriteTo.Debug()
```

**Production Environment**:
```csharp
.WriteTo.ApplicationInsights(
    aiConnectionString,
    new TraceTelemetryConverter(),
    LogEventLevel.Information
)
.WriteTo.File(
    path: "logs/potictac-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7
)
```

### Enrichers

1. **CorrelationEnricher**: Adds correlation ID from HTTP context for request tracing
2. **ThreadId**: Identifies which thread processed the log event
3. **ProcessId**: Identifies which process instance (useful in multi-instance deployments)
4. **FromLogContext**: Captures ambient context properties (e.g., user ID, request path)

### Custom Telemetry Initializer

`CustomTelemetryInitializer` enriches Application Insights telemetry with:
- Cloud role name (service name)
- Cloud role instance (hostname/container ID)
- User authentication status
- Request context

## Consequences

### Positive

✅ **Structured Data**: Logs are queryable JSON objects, not text strings  
✅ **Powerful Queries**: Application Insights KQL queries like `traces | where customDimensions.PlayerName == "Alice"`  
✅ **Performance**: Asynchronous sinks don't block request threads  
✅ **Correlation**: Trace entire request lifecycle across services (future microservices)  
✅ **Rich Context**: Automatic enrichment with thread, process, request info  
✅ **Environment-Specific**: Different sinks for dev (console) vs. prod (App Insights)  
✅ **Exception Tracking**: Full stack traces and exception details captured  
✅ **Minimal Code**: Simple `Log.Information("Message", property1, property2)` syntax  

### Negative

⚠️ **Learning Curve**: Developers must learn structured logging syntax (`{PropertyName}` not string interpolation)  
⚠️ **Configuration Complexity**: More setup than default `ILogger`  
⚠️ **Package Dependencies**: Additional NuGet packages (Serilog.AspNetCore, Serilog.Sinks.ApplicationInsights)  
⚠️ **Application Insights Costs**: Potential costs if log volume exceeds 5GB/month (unlikely for this app)  

### Trade-offs

- **Setup Complexity vs. Capability**: More initial setup for powerful production insights
- **String Interpolation vs. Structured Properties**: Slightly different syntax for much better queryability

## Alternatives Considered

### 1. Built-in ASP.NET Core ILogger
**Pros**: Zero configuration, built-in, simple API, familiar to all .NET developers  
**Cons**: String-based logging (not structured), poor cloud integration, basic console output, no built-in Application Insights sink  
**Why Rejected**: Lacks structured logging capabilities needed for production observability; difficult to query logs by specific properties

### 2. NLog
**Pros**: Mature, flexible configuration (XML/JSON), multiple targets, large ecosystem  
**Cons**: XML configuration is verbose, less ergonomic API, fewer built-in enrichers, Application Insights integration less seamless  
**Why Rejected**: Configuration is cumbersome compared to Serilog's fluent API; Serilog has better Application Insights integration

### 3. Log4Net
**Pros**: Very mature, stable, widely used in enterprise Java/C# applications  
**Cons**: XML configuration required, dated API, poor structured logging support, no async sinks  
**Why Rejected**: Dated framework designed for file/database logging, not cloud-native telemetry

### 4. Application Insights SDK Directly
**Pros**: Native Azure integration, no intermediate library, full telemetry features  
**Cons**: Tightly coupled to Azure, no local development experience, verbose API, no console output  
**Why Rejected**: Makes local development difficult; no console output for debugging; vendor lock-in

### 5. Console.WriteLine / Debug.WriteLine
**Pros**: Zero dependencies, fastest possible, no configuration  
**Cons**: No structure, no persistence, disappears on restart, no production value  
**Why Rejected**: Not production-ready; logs lost on app restart; impossible to query or analyze

## Implementation Notes

### Logging Best Practices

**DO**:
```csharp
// Structured logging with properties
_logger.LogInformation("Player {PlayerName} won game in {Moves} moves", playerName, moveCount);

// Contextual enrichment
using (_logger.BeginScope(new Dictionary<string, object> { ["GameId"] = gameId }))
{
    _logger.LogInformation("Game started");
    _logger.LogInformation("Player joined");  // Automatically includes GameId
}
```

**DON'T**:
```csharp
// String interpolation loses structure
_logger.LogInformation($"Player {playerName} won game in {moveCount} moves");

// Concatenation loses query ability
_logger.LogInformation("Player " + playerName + " won game");
```

### Log Levels

| Level | Purpose | Example |
|-------|---------|---------|
| **Trace** | Very detailed diagnostics | `"Evaluating AI move for position {Position}"` |
| **Debug** | Developer insights | `"Game state: {BoardState}"` |
| **Information** | General flow | `"Player {Name} joined game {GameId}"` |
| **Warning** | Unexpected but handled | `"Player disconnected, auto-forfeiting in 30s"` |
| **Error** | Failures requiring investigation | `"Failed to save player stats: {Exception}"` |
| **Critical** | Application-wide failures | `"Storage connection lost, entering read-only mode"` |

### Request Logging Enrichment

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});
```

Every HTTP request automatically logs:
- **Method**: GET, POST, PUT, DELETE
- **Path**: /api/players/Alice
- **Status Code**: 200, 404, 500
- **Duration**: 42ms
- **Request Host**, **Client IP**, **User Agent**

### Application Insights KQL Queries

```kql
// Find all errors in the last hour
traces
| where timestamp > ago(1h)
| where severityLevel >= 3  // Error or Critical
| order by timestamp desc

// Analyze API endpoint performance
requests
| where timestamp > ago(24h)
| summarize avgDuration=avg(duration), count() by name
| order by avgDuration desc

// Find games won by specific player
traces
| where customDimensions.PlayerName == "Alice"
| where message contains "won game"
| project timestamp, message, customDimensions
```

### Log File Management

**Production File Logs**:
- **Location**: `logs/potictac-YYYYMMDD.log`
- **Rolling**: Daily at midnight
- **Retention**: Last 7 days only (automatic cleanup)
- **Format**: JSON for machine parsing

**Purpose**: Backup logs if Application Insights is unreachable; local troubleshooting on VMs

## Cost Considerations

**Application Insights Costs**:
- **Free Tier**: 5 GB/month ingestion
- **Estimated Usage**: ~100-200 MB/month for typical traffic
  - ~10,000 requests/day × 5KB/request = 50MB/day = 1.5GB/month
- **Cost if Exceeded**: $2.30 per GB over 5GB (unlikely to exceed)

**Recommendations**:
1. Set Application Insights daily cap at 1 GB/day
2. Filter out `Trace` and `Debug` levels in production
3. Sample high-volume events if traffic spikes

## Testing Considerations

**Unit Tests**: Use `ILogger<T>` abstraction; inject `NullLogger<T>` in tests  
**Integration Tests**: Capture logs with `TestLoggerProvider` to assert on log messages  
**Load Tests**: Disable Serilog or use `NullSink` to eliminate logging overhead

## References

- [Serilog Documentation](https://serilog.net/)
- [Serilog ASP.NET Core Integration](https://github.com/serilog/serilog-aspnetcore)
- [Serilog Application Insights Sink](https://github.com/serilog-contrib/serilog-sinks-applicationinsights)
- [Application Insights KQL Reference](https://learn.microsoft.com/azure/data-explorer/kusto/query/)

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If Application Insights costs exceed $10/month

## Related ADRs
- [ADR-006: Use Azure App Service for Hosting](./006-azure-app-service-hosting.md)
