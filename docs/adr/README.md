# Architectural Decision Records (ADRs)

This directory contains Architectural Decision Records (ADRs) for the PoTicTac project. ADRs document significant architectural decisions made during the development process, including the context, rationale, consequences, and alternatives considered.

## ADR Index

| ADR # | Title | Status | Date |
|-------|-------|--------|------|
| [001](./001-blazor-webassembly.md) | Use Blazor WebAssembly for Client | Accepted | 2025-08-02 |
| [002](./002-azure-table-storage.md) | Use Azure Table Storage for Persistence | Accepted | 2025-08-02 |
| [003](./003-serilog-structured-logging.md) | Use Serilog for Structured Logging | Accepted | 2025-08-02 |
| [004](./004-signalr-realtime-multiplayer.md) | Use SignalR for Real-Time Multiplayer | Accepted | 2025-08-02 |
| [005](./005-vertical-slice-architecture.md) | Adopt Vertical Slice Architecture | Accepted | 2025-08-02 |
| [006](./006-azure-app-service-hosting.md) | Use Azure App Service for Hosting | Accepted | 2025-08-02 |
| [007](./007-minimax-ai-strategy.md) | Implement Minimax Algorithm for Hard AI | Accepted | 2025-08-02 |
| [008](./008-6x6-board-4-in-a-row.md) | 6x6 Board with 4-in-a-Row Victory | Accepted | 2025-08-02 |

## ADR Template

Each ADR follows this structure:

```markdown
# ADR-XXX: [Title]

## Status
[Proposed | Accepted | Deprecated | Superseded]

## Context
What is the issue that we're seeing that is motivating this decision or change?

## Decision
What is the change that we're proposing and/or doing?

## Consequences
What becomes easier or more difficult to do because of this change?

## Alternatives Considered
What other options did we consider?
```

## References
- [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) by Michael Nygard
- [ADR GitHub Organization](https://adr.github.io/)
