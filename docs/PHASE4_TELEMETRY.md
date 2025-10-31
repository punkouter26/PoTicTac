# Phase 4: Telemetry, Logging, & Production Debugging

## Overview

This document provides comprehensive guidance for the telemetry, logging, and production debugging infrastructure implemented in PoTicTac.

---

## Table of Contents

1. [Structured Logging with Serilog](#structured-logging-with-serilog)
2. [Application Insights Telemetry](#application-insights-telemetry)
3. [Custom Metrics and Traces](#custom-metrics-and-traces)
4. [Production Diagnostics](#production-diagnostics)
5. [KQL Query Library](#kql-query-library)
6. [Configuration Guide](#configuration-guide)
7. [Testing and Verification](#testing-and-verification)

---

## Structured Logging with Serilog

### Architecture

PoTicTac uses **Serilog** for all server-side structured logging with environment-specific sinks:

- **Development**: Console + Debug sinks
- **Production**: Application Insights + File sinks

### Configuration

#### appsettings.json (Production)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "PoTicTacServer": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/potictac-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithThreadId", "WithProcessId" ]
  }
}
```

#### appsettings.Development.json
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Debug"
      }
    ]
  }
}
```

### Log Enrichment

All logs are enriched with:
- **CorrelationId**: Trace ID from Activity (OpenTelemetry)
- **SpanId**: Current span ID
- **UserId**: Authenticated user ID or "anonymous"
- **TenantId**: Tenant identifier (multi-tenant support)
- **SessionId**: User session ID
- **ClientIp**: Client IP address
- **RequestPath**: HTTP request path
- **ThreadId**: Thread identifier
- **ProcessId**: Process identifier

### Usage Examples

```csharp
using Microsoft.Extensions.Logging;

public class StatisticsController : ControllerBase
{
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(ILogger<StatisticsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        _logger.LogInformation("Fetching statistics for user {UserId}", userId);
        
        try
        {
            var stats = await _service.GetStatsAsync();
            _logger.LogDebug("Retrieved {Count} statistics records", stats.Count);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch statistics for user {UserId}", userId);
            throw;
        }
    }
}
```

---

## Application Insights Telemetry

### Server-Side Configuration

#### Packages Required
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
<PackageReference Include="Microsoft.ApplicationInsights.SnapshotCollector" Version="1.4.6" />
<PackageReference Include="Microsoft.ApplicationInsights.Profiler.AspNetCore" Version="2.6.0" />
<PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
```

#### Program.cs Configuration
```csharp
// Configure Application Insights
var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(aiConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = aiConnectionString;
        options.EnableAdaptiveSampling = true;
        options.EnablePerformanceCounterCollectionModule = true;
        options.EnableQuickPulseMetricStream = true;
    });

    // Register custom telemetry initializer
    builder.Services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
}
```

### Client-Side Configuration

Application Insights JavaScript SDK is integrated via `index.html` for:
- Page view tracking
- Client-side exception tracking
- Performance metrics
- Operation ID correlation

**Note**: Replace `%APPLICATIONINSIGHTS_CONNECTION_STRING%` with actual connection string during deployment.

### Custom Telemetry Initializer

The `CustomTelemetryInitializer` enriches all telemetry with:
- **UserRole**: User's role (guest, player, admin)
- **GameMode**: Current game mode (singleplayer, multiplayer)
- **Difficulty**: AI difficulty level
- **Environment**: ASPNETCORE_ENVIRONMENT value
- **AppVersion**: Application version

---

## Custom Metrics and Traces

### ActivitySource for Distributed Tracing

Use `PoTicTacTelemetry.ActivitySource` to create custom spans:

```csharp
using System.Diagnostics;
using PoTicTacServer.Telemetry;

public class GameService
{
    public async Task StartGameAsync()
    {
        using var activity = PoTicTacTelemetry.ActivitySource.StartActivity("StartGame");
        activity?.SetTag("gameMode", "singleplayer");
        activity?.SetTag("difficulty", "hard");
        
        try
        {
            // Business logic here
            PoTicTacTelemetry.GamesStarted.Add(1, 
                new KeyValuePair<string, object?>("mode", "singleplayer"),
                new KeyValuePair<string, object?>("difficulty", "hard")
            );
            
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
```

### Custom Metrics

Available metrics in `PoTicTacTelemetry`:

| Metric | Type | Description |
|--------|------|-------------|
| `potictac.games.started` | Counter | Total games started |
| `potictac.games.completed` | Counter | Total games completed |
| `potictac.moves.played` | Counter | Total moves made |
| `potictac.game.duration` | Histogram | Game duration (seconds) |
| `potictac.ai.calculation_time` | Histogram | AI calculation time (ms) |
| `potictac.moves.per_game` | Histogram | Moves per completed game |
| `potictac.wins.player` | Counter | Player wins |
| `potictac.wins.ai` | Counter | AI wins |
| `potictac.games.draws` | Counter | Draw games |
| `potictac.multiplayer.games.started` | Counter | Multiplayer games |
| `potictac.errors.game` | Counter | Game errors |
| `potictac.errors.storage` | Counter | Storage errors |

### Usage Example
```csharp
// Record game completed
var stopwatch = Stopwatch.StartNew();
// ... game logic ...
stopwatch.Stop();

PoTicTacTelemetry.GamesCompleted.Add(1);
PoTicTacTelemetry.GameDuration.Record(stopwatch.Elapsed.TotalSeconds);
PoTicTacTelemetry.MovesPerGame.Record(moveCount);

if (winner == PlayerType.X)
{
    PoTicTacTelemetry.PlayerWins.Add(1);
}
else if (winner == PlayerType.O)
{
    PoTicTacTelemetry.AiWins.Add(1);
}
```

---

## Production Diagnostics

### Snapshot Debugger

**Purpose**: Captures full debug snapshots (locals, call stack) when exceptions occur in production.

#### Configuration
```json
{
  "ApplicationInsights": {
    "EnableSnapshotDebugger": true
  },
  "SnapshotDebugger": {
    "IsEnabled": true,
    "ThresholdForSnapshotting": 1,
    "MaximumSnapshotsRequired": 3,
    "SnapshotsPerDayLimit": 30
  }
}
```

#### Azure Portal Setup
1. Navigate to Application Insights resource
2. Go to **Snapshot Debugger** blade
3. Enable Snapshot Debugger
4. Configure collection rules
5. Deploy application with updated settings

### Application Insights Profiler

**Purpose**: Captures detailed performance traces (CPU, memory) for slow requests.

#### Configuration
```json
{
  "ApplicationInsights": {
    "EnableProfiler": true
  },
  "ServiceProfiler": {
    "IsEnabled": true,
    "CollectionPlan": "Automatic"
  }
}
```

#### Azure Portal Setup
1. Navigate to Application Insights resource
2. Go to **Performance** blade
3. Click **Profiler** tab
4. Enable Profiler
5. Configure profiling sessions (typically 2-minute sessions every hour)

---

## KQL Query Library

All KQL queries are located in `docs/KQL/` folder:

### 1. user-activity.kql
- Active users and sessions (last 7 days)
- Session duration analysis
- User engagement metrics

### 2. server-performance.kql
- Top 10 slowest requests
- Performance trends over time
- Performance by result code

### 3. server-stability.kql
- Overall error rate
- Error rate by endpoint
- 5xx vs 4xx errors

### 4. client-side-errors.kql
- Top client-side exceptions
- Exception trends by browser
- Exception impact on UX

### 5. e2e-trace-funnel.kql
- End-to-end operation tracing
- PageView → Request → Dependency correlation
- Success rate funnel

### 6. custom-game-metrics.kql
- Game completion rate
- Average game duration
- Win rate analysis
- AI performance statistics

### 7. custom-events.kql
- Game lifecycle events
- AI difficulty distribution
- Custom error tracking

---

## Configuration Guide

### Local Development Setup

1. **No Application Insights Required**
   - Logs go to Console and Debug output
   - No connection string needed

2. **Test with Azurite**
   ```bash
   # Start Azurite
   azurite --silent --location ./AzuriteConfig --debug ./AzuriteConfig/debug.log
   ```

3. **Run Application**
   ```bash
   dotnet run --project PoTicTacServer
   ```

4. **View Logs**
   - Console output shows all logs
   - File logs in `logs/potictac-{date}.log`

### Production Azure Deployment

1. **Create Application Insights Resource**
   ```bash
   az monitor app-insights component create \
     --app potictac-appinsights \
     --location eastus \
     --resource-group potictac-rg \
     --application-type web
   ```

2. **Get Connection String**
   ```bash
   az monitor app-insights component show \
     --app potictac-appinsights \
     --resource-group potictac-rg \
     --query connectionString -o tsv
   ```

3. **Configure App Service**
   ```bash
   az webapp config appsettings set \
     --name potictac-app \
     --resource-group potictac-rg \
     --settings ApplicationInsights__ConnectionString="<connection-string>"
   ```

4. **Enable Snapshot Debugger** (Optional)
   ```bash
   az webapp config appsettings set \
     --name potictac-app \
     --resource-group potictac-rg \
     --settings \
       ApplicationInsights__EnableSnapshotDebugger=true \
       SnapshotDebugger__IsEnabled=true
   ```

5. **Enable Profiler** (Optional)
   ```bash
   az webapp config appsettings set \
     --name potictac-app \
     --resource-group potictac-rg \
     --settings \
       ApplicationInsights__EnableProfiler=true \
       ServiceProfiler__IsEnabled=true
   ```

---

## Testing and Verification

### Local Testing

1. **Verify Serilog Output**
   ```csharp
   // Add to any controller
   _logger.LogInformation("Test log with {Property}", "value");
   ```
   
   Expected console output:
   ```
   [12:34:56 INF] PoTicTacServer.Controllers.StatisticsController Test log with value
   ```

2. **Test Custom Metrics**
   ```csharp
   PoTicTacTelemetry.GamesStarted.Add(1);
   ```
   
   Verify in Application Insights (if configured) or local metrics endpoint.

3. **Test ActivitySource**
   ```csharp
   using var activity = PoTicTacTelemetry.ActivitySource.StartActivity("TestOperation");
   activity?.SetTag("test", "value");
   ```

### Production Verification

1. **Check Application Insights Live Metrics**
   - Open Application Insights in Azure Portal
   - Navigate to **Live Metrics**
   - Verify requests, dependencies, and custom metrics appear in real-time

2. **Run KQL Queries**
   - Open **Logs** blade in Application Insights
   - Run queries from `docs/KQL/` folder
   - Verify data is being collected

3. **Test Snapshot Debugger**
   - Trigger an exception in production
   - Wait 5-10 minutes
   - Check **Snapshot Debugger** blade for captured snapshots

4. **Test Profiler**
   - Generate load on slow endpoints
   - Wait for next profiling session
   - Check **Performance → Profiler** for traces

---

## Troubleshooting

### No Logs in Application Insights

**Problem**: Logs not appearing in Azure
**Solution**: 
- Verify connection string is set correctly
- Check `appsettings.json` has `Serilog.Sinks.ApplicationInsights` configured
- Ensure environment is not "Development"

### Custom Metrics Not Showing

**Problem**: Custom metrics (Counter, Histogram) not in Application Insights
**Solution**:
- Verify `AddApplicationInsightsTelemetry` is called in Program.cs
- Check metrics are actually being recorded in code
- Wait 1-2 minutes for metrics to appear (ingestion delay)

### Snapshot Debugger Not Working

**Problem**: Snapshots not being captured
**Solution**:
- Verify Snapshot Debugger is enabled in App Service
- Check App Service is using correct Application Insights resource
- Ensure exceptions are actually being thrown
- Check daily snapshot limit hasn't been reached

---

## Best Practices

1. **Use Structured Logging**
   ```csharp
   // Good: Structured
   _logger.LogInformation("User {UserId} started game {GameId}", userId, gameId);
   
   // Bad: String interpolation
   _logger.LogInformation($"User {userId} started game {gameId}");
   ```

2. **Add Context to Activities**
   ```csharp
   using var activity = PoTicTacTelemetry.ActivitySource.StartActivity("GameOperation");
   activity?.SetTag("gameId", gameId);
   activity?.SetTag("playerId", playerId);
   activity?.SetTag("difficulty", difficulty);
   ```

3. **Record Business Metrics**
   ```csharp
   // Always record key business events
   PoTicTacTelemetry.GamesStarted.Add(1);
   PoTicTacTelemetry.GameDuration.Record(duration.TotalSeconds);
   ```

4. **Use Appropriate Log Levels**
   - **Trace**: Very detailed, typically only in Development
   - **Debug**: Debugging information, useful in Development
   - **Information**: General flow, business events
   - **Warning**: Abnormal but expected situations
   - **Error**: Errors and exceptions
   - **Critical**: Critical failures requiring immediate attention

5. **Minimize PII in Logs**
   - Never log passwords, tokens, or sensitive data
   - Use user IDs instead of names/emails when possible
   - Redact sensitive information

---

## Summary

Phase 4 provides comprehensive telemetry, logging, and diagnostics for PoTicTac:

✅ **Structured Logging**: Serilog with environment-specific sinks
✅ **Application Insights**: Client and server telemetry with correlation
✅ **Custom Metrics**: 15+ business metrics for game analytics
✅ **Custom Traces**: ActivitySource for distributed tracing
✅ **Enrichment**: Automatic correlation IDs, user context, custom properties
✅ **Production Diagnostics**: Snapshot Debugger and Profiler ready
✅ **KQL Library**: 7 query files for common analysis scenarios

**Next Steps**:
1. Deploy to Azure and configure Application Insights connection string
2. Enable Snapshot Debugger and Profiler in production
3. Monitor Live Metrics and run KQL queries
4. Set up alerts for error rates and performance degradation

---

**Document Version**: 1.0  
**Last Updated**: October 31, 2025  
**Author**: AI Coding Agent
