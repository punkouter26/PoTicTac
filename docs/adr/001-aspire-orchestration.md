# ADR-001: Use .NET Aspire for Orchestration

## Status
Accepted

## Date
2026-01-08

## Context

PoTicTac requires:
- Local development with Azure Table Storage (via Azurite emulator)
- Consistent configuration between local and cloud environments
- Observability (logs, traces, metrics) out of the box
- Health checks and resilience patterns
- E2E test orchestration

Previously, developers had to manually:
1. Start Azurite container
2. Configure connection strings in multiple places
3. Set up Application Insights separately
4. Run E2E tests with custom scripts

## Decision

Adopt **.NET Aspire 9.2.1** as the orchestration layer with:

- **AppHost project** (`Po.TicTac.AppHost`) as the single entry point
- **ServiceDefaults project** (`Po.TicTac.ServiceDefaults`) for shared cross-cutting concerns
- **Aspire.Azure.Data.Tables** for Table Storage integration
- **Azurite emulator** with persistent volume for local development

```csharp
// AppHost Program.cs
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(emulator => emulator
        .WithDataVolume("potictac-storage")
        .WithLifetime(ContainerLifetime.Persistent));

var tables = storage.AddTables("tables");

builder.AddProject<Projects.Po_TicTac_Api>("api")
    .WithReference(tables)
    .WaitFor(tables);
```

## Consequences

### Positive
- **Single command startup**: `dotnet run --project src/Po.TicTac.AppHost`
- **Automatic service discovery**: No manual connection string management
- **Built-in dashboard**: Real-time logs, traces, and metrics at `http://localhost:15888`
- **Health checks included**: `/health` and `/alive` endpoints auto-configured
- **Production parity**: Same resource references work with Azure Developer CLI (`azd up`)

### Negative
- Requires Docker Desktop for Azurite emulator
- Additional learning curve for developers unfamiliar with Aspire
- AppHost project adds ~2-3 seconds to initial startup

### Neutral
- E2E tests can be integrated via `AddNpmApp()` but require separate npm install
