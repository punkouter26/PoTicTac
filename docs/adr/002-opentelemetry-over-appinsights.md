# ADR-002: OpenTelemetry over Application Insights SDK

## Status
Accepted

## Date
2026-01-08

## Context

The application previously used the legacy Application Insights SDK:
- `Microsoft.ApplicationInsights.AspNetCore`
- `Microsoft.ApplicationInsights.SnapshotCollector`
- `Microsoft.ApplicationInsights.Profiler.AspNetCore`
- `Serilog.Sinks.ApplicationInsights`

This approach had several issues:
1. **Vendor lock-in**: Tightly coupled to Azure Application Insights
2. **Duplicate telemetry**: Multiple SDKs sending similar data
3. **Maintenance burden**: 4 separate packages to update
4. **Incompatibility**: Legacy SDK conflicts with .NET Aspire's OpenTelemetry

## Decision

Replace Application Insights SDK with **OpenTelemetry** + **Azure Monitor Exporter**:

```csharp
// ServiceDefaults/Extensions.cs
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

// When APPLICATIONINSIGHTS_CONNECTION_STRING is set:
builder.Services.AddOpenTelemetry().UseAzureMonitor();
```

## Consequences

### Positive
- **Vendor neutral**: Can switch to Jaeger, Zipkin, or other backends
- **Single SDK**: OpenTelemetry handles traces, metrics, and logs
- **Aspire integration**: Works seamlessly with Aspire Dashboard locally
- **Modern standard**: OpenTelemetry is the CNCF standard for observability
- **Reduced packages**: 4 packages → 1 (`Azure.Monitor.OpenTelemetry.AspNetCore`)

### Negative
- Some Application Insights features not available (Snapshot Debugger, Profiler)
- Requires `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable
- Different query syntax in Azure Monitor vs Application Insights classic

### Migration Path
1. ✅ Remove legacy Application Insights packages
2. ✅ Add `Azure.Monitor.OpenTelemetry.AspNetCore`
3. ✅ Configure via ServiceDefaults
4. ✅ Delete `CustomTelemetryInitializer.cs`
5. ⏳ Update KQL queries in `docs/KQL/` if needed
