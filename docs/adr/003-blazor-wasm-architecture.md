# ADR-003: Blazor WebAssembly with API Host

## Status
Accepted

## Date
2026-01-08

## Context

PoTicTac is a single-page game application that needs:
- Rich interactive UI (game board, animations)
- Real-time multiplayer via SignalR
- Offline-capable gameplay against AI
- Statistics persistence to backend

Options considered:
1. **Blazor Server**: Server-rendered with SignalR connection
2. **Blazor WebAssembly (Standalone)**: Client-only with separate API
3. **Blazor WebAssembly (Hosted)**: Client + API in single deployment

## Decision

Use **Blazor WebAssembly hosted by ASP.NET Core API**:

```
src/
├── Po.TicTac.Api/          # ASP.NET Core host
│   └── References Po.TicTac.Client
├── Po.TicTac.Client/       # Blazor WebAssembly
└── Po.TicTac.Shared/       # Shared DTOs
```

The API project:
- Serves the Blazor WASM files via `UseBlazorFrameworkFiles()`
- Provides REST endpoints for statistics
- Hosts SignalR hub for multiplayer
- Handles health checks and telemetry

## Consequences

### Positive
- **Single deployment unit**: One container/app service hosts everything
- **Shared code**: DTOs in `Po.TicTac.Shared` used by both client and server
- **Offline AI**: Game logic runs entirely in browser (no server round-trips)
- **Reduced latency**: Static assets served from same origin (no CORS)
- **Simplified auth**: Cookie/token auth works seamlessly

### Negative
- **Initial load time**: ~2-3MB download for .NET runtime in browser
- **SEO limitations**: Client-rendered content not indexed (acceptable for game)
- **Build complexity**: Client must build before API can reference it

### Mitigations
- Compression enabled for WASM files
- Lazy loading for non-critical components
- PWA manifest for installable experience
