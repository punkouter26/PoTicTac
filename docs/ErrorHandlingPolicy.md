# PoTicTac - Error Handling Policy

## Overview

This document describes how errors are handled throughout the PoTicTac application, from the API backend through to the React frontend. The system follows an **offline-first resilience pattern** where the game remains fully playable even when the API is unavailable.

---

## Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                        React Frontend                           │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│   │ UI Display  │←─│ Error State │←─│ Service Layer (api.ts)  │ │
│   └─────────────┘  └─────────────┘  └─────────────────────────┘ │
└───────────────────────────────────────────────────────────────┬─┘
                                                                │
                              HTTP/REST                         │
                                                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        .NET API Backend                         │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│   │  Endpoints  │──│  MediatR    │──│ StorageService          │ │
│   └─────────────┘  └─────────────┘  └─────────────────────────┘ │
│                           │                                     │
│                           ▼                                     │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │              Serilog + Application Insights              │   │
│   └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

---

## Error Categories

### 1. Client-Side Errors (React)

| Error Type | Handling | User Experience |
|------------|----------|-----------------|
| Network failure | Graceful degradation | "API offline, playing locally" |
| API 404 | Return default stats | Seamless (new player) |
| API 500 | Log warning, continue | Game continues, stats not saved |
| Invalid game move | Prevent action | Cell remains unclickable |

### 2. Server-Side Errors (.NET)

| Error Type | HTTP Code | Logging | Response |
|------------|-----------|---------|----------|
| Invalid input | 400 | Warning | ProblemDetails JSON |
| Resource not found | 404 | Info | ProblemDetails JSON |
| Storage failure | 500 | Error + Exception | ProblemDetails JSON |
| Unhandled exception | 500 | Error + Stack trace | Generic error |

---

## Frontend Error Handling

### API Service (`api.ts`)

The API service wraps all fetch calls with error handling:

```typescript
async function fetchApi<T>(url: string, options?: RequestInit): Promise<T | null> {
  try {
    const response = await fetch(`${API_BASE}${url}`, { ...options });

    if (!response.ok) {
      console.warn(`API error: ${response.status} ${response.statusText}`);
      return null;  // Return null, don't throw
    }

    if (response.status === 204) {
      return null;  // No content
    }

    return await response.json();
  } catch (error) {
    console.warn('API request failed, app continues in offline mode:', error);
    return null;  // Network error - graceful degradation
  }
}
```

**Key Principles**:
1. **Never throw** - Return `null` instead
2. **Log warnings** - Console warnings for debugging
3. **Offline-first** - Game works without API

### Component Error Handling

```typescript
// LeaderboardPage.tsx
useEffect(() => {
  async function fetchLeaderboard() {
    try {
      const data = await StatisticsApi.getLeaderboard(10);
      if (data) {
        setLeaderboard(data);
      } else {
        setError('Unable to load leaderboard. API may be offline.');
      }
    } catch (err) {
      setError('Failed to load leaderboard');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }
  fetchLeaderboard();
}, []);
```

### Local Storage Fallback

```typescript
// When API fails, use local storage
try {
  await StatisticsApi.savePlayerStats(playerName, stats);
} catch (error) {
  console.error('Failed to save stats to API:', error);
  // Stats already saved to LocalStorage as backup
  LocalStorageService.setPlayerStats(playerName, stats);
}
```

---

## Backend Error Handling

### Serilog Configuration

```csharp
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithThreadId()
        .Enrich.WithProcessId()
        .Enrich.With(new CorrelationEnricher(...));

    // Development: Console + Debug
    // Production: Console + File + Application Insights
});
```

### Request Logging

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress);
    };
});
```

### Handler Error Handling

```csharp
public async Task<IEnumerable<PlayerStatsDto>> Handle(GetLeaderboardQuery request, CancellationToken ct)
{
    logger.LogInformation("Retrieving leaderboard with limit: {Limit}", request.Limit);

    try
    {
        var result = await cache.GetOrCreateAsync(...);
        logger.LogInformation("Retrieved {Count} players", result?.Count ?? 0);
        return result ?? [];
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving leaderboard");
        throw;  // Let global handler catch it
    }
}
```

---

## Monitoring & Alerting

### Application Insights Integration

```csharp
// OpenTelemetry with Azure Monitor
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}
```

### What's Logged

| Log Level | Content | Destination |
|-----------|---------|-------------|
| Debug | Cache hits/misses | Dev console only |
| Information | Request start/end, counts | All sinks |
| Warning | API errors, missing data | All sinks |
| Error | Exceptions with stack trace | All sinks + Alerts |

### Log Format (Production)

```
2026-02-05 14:22:33.456 +00:00 [ERR] [StorageService] Error retrieving player stats 
{"PlayerName":"TestUser","RequestId":"abc123","ClientIp":"1.2.3.4"}
Azure.RequestFailedException: The specified resource does not exist.
   at Azure.Data.Tables.TableClient.GetEntityAsync[T]...
```

---

## Health Check Errors

### Health Check Endpoint

```csharp
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = _ => false  // Liveness: just returns if app runs
});
```

### Storage Health Check

```csharp
public class StorageHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        try
        {
            await _tableClient.QueryAsync<TableEntity>(maxPerPage: 1).FirstOrDefaultAsync();
            return HealthCheckResult.Healthy("Azure Table Storage is accessible");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Table Storage is not accessible", ex);
        }
    }
}
```

---

## Error Response Examples

### 404 Not Found

```http
GET /api/players/unknown-player/stats

HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Player 'unknown-player' not found"
}
```

### 400 Bad Request

```http
PUT /api/players//stats

HTTP/1.1 400 Bad Request
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Player name cannot be empty"
}
```

---

## Best Practices Summary

1. **Never crash the game** - Errors should degrade gracefully
2. **Log everything meaningful** - Use structured logging with context
3. **User-friendly messages** - Technical details in console, friendly messages in UI
4. **Offline-first** - LocalStorage backup for all critical data
5. **Correlation IDs** - Track requests across frontend and backend
6. **Health checks** - Proactive monitoring of dependencies

---

## Related Documentation

- [DataWorkflow.mmd](./DataWorkflow.mmd) - Request flow diagrams
- [ApiContract.md](./api/ApiContract.md) - Error response formats
- [DeploymentPipeline.md](./ci-cd/DeploymentPipeline.md) - Health check in CI/CD
