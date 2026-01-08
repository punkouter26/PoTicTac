# PoTicTac - AI Coding Agent Instructions

## Project Overview
A 6x6 Tic Tac Toe game requiring 4-in-a-row to win. Built with **Blazor WebAssembly** client hosted by **ASP.NET Core** API, orchestrated by **.NET Aspire**.

## Core Standards & SDKs
- **Tech Stack**: Target .NET 10 and C# 14 exclusively
- **Unified Naming**: Use `PoTicTac` as master identifier for Azure Resource Groups
- **Namespaces**: Apply consistent `Po.TicTac.*` prefix across all projects
- **Package Management**: Central Package Management (CPM) in `Directory.Packages.props`
- **Quality Gates**: Mandatory `<TreatWarningsAsErrors>true` for all projects

## Architecture

### Project Structure (Aspire-Enabled)
```
src/
├── Po.TicTac.AppHost/      # Aspire orchestration (start here for local dev)
├── Po.TicTac.ServiceDefaults/  # Shared OpenTelemetry, health checks, resilience
├── Po.TicTac.Api/          # ASP.NET Core backend hosting Blazor WASM
├── Po.TicTac.Client/       # Blazor WebAssembly frontend
└── Po.TicTac.Shared/       # Shared DTOs/models
tests/
├── Po.TicTac.UnitTests/    # Fast unit tests (xUnit)
├── Po.TicTac.IntegrationTests/  # API integration tests
└── Po.TicTac.E2ETests/     # Playwright E2E tests (npm)
api/                        # TypeSpec API contracts
docs/
├── adr/                    # Architecture Decision Records
└── KQL/                    # Azure Monitor queries
infra/                      # Bicep infrastructure templates
```

### Running with Aspire
```bash
# Start the orchestrated app (preferred method)
dotnet run --project src/Po.TicTac.AppHost

# Or use Aspire CLI
aspire run
```
The AppHost starts Azurite emulator automatically with persistent storage volume.
Access the Aspire Dashboard for logs, traces, and metrics.

### Key Patterns

**ServiceDefaults Integration** - All services call `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();  // OpenTelemetry, health checks, service discovery
// ... service configuration
var app = builder.Build();
app.MapDefaultEndpoints();     // /health, /alive endpoints
```

**Aspire Resource References** - Use `WithReference()` for service discovery:
```csharp
// In AppHost Program.cs
var tables = storage.AddTables("tables");
builder.AddProject<Projects.Po_TicTac_Api>("api")
    .WithReference(tables)
    .WaitFor(tables);
```

**Azure Table Client** - Use Aspire's integration instead of manual connection strings:
```csharp
builder.AddAzureTableServiceClient("tables");  // Connection injected by AppHost
```

## Game Constants
- **Board size**: 6x6 (defined in `GameBoardState.BoardSize`)
- **Win condition**: 4 in a row (defined in `WinChecker.WinLength`)

## API Feature Organization (Vertical Slices)
Features organized in `Features/{Domain}/` with MediatR CQRS:
```
Features/Statistics/GetLeaderboard.cs  # Query + Handler + Endpoint in one file
```
Each feature exports `Map{FeatureName}()` registered in `Program.cs`.

## AI Strategy Pattern
AI difficulty uses Strategy pattern in `Services/AIStrategies/`:
- `EasyAIStrategy` - Random moves with 30% blocking
- `MediumAIStrategy` - Threat detection
- `HardAIStrategy` - Minimax with alpha-beta pruning (depth 4)

## Testing Commands
```bash
dotnet test tests/Po.TicTac.UnitTests/         # Fast unit tests
dotnet test tests/Po.TicTac.IntegrationTests/  # Requires storage
dotnet format PoTicTac.sln              # Code formatting

# E2E tests (Playwright) - run via AppHost or directly:
cd tests/Po.TicTac.E2ETests && npm test
```

## Azure Deployment
Uses **Azure Developer CLI (azd)** with Aspire manifest:
```bash
azd up  # Generates manifest from AppHost, provisions infrastructure, deploys
```

The `azure.yaml` points to the AppHost project. During `azd up`, Aspire generates a deployment manifest describing all resources (API, storage, etc.) which azd translates to Azure infrastructure.

## Infrastructure & Observability
- **Zero-Trust**: Use Azure Key Vault via Managed Identity
- **Telemetry**: OpenTelemetry with Azure Monitor exporter (replaces legacy Application Insights SDK)
- **Health Checks**: Deep readiness probes verify all backing services (`/health`, `/alive`)
- **Local Validation**: Run `az bicep build` before commits
- **Aspire Dashboard**: Access locally for traces, logs, and metrics visualization

## Key Files Reference
| Pattern | Example File |
|---------|--------------|
| AppHost orchestration | `src/Po.TicTac.AppHost/Program.cs` |
| ServiceDefaults | `src/Po.TicTac.ServiceDefaults/Extensions.cs` |
| API feature | `src/Po.TicTac.Api/Features/Statistics/GetLeaderboard.cs` |
| AI strategy | `src/Po.TicTac.Client/Services/AIStrategies/HardAIStrategy.cs` |
| Razor component | `src/Po.TicTac.Client/Components/GameBoard.razor` |
| TypeSpec contracts | `api/main.tsp` |
| ADRs | `docs/adr/` |
