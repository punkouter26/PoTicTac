# Architecture Decision Records

This folder contains Architecture Decision Records (ADRs) documenting significant technical decisions made in the PoTicTac project.

## What is an ADR?

An ADR captures an important architectural decision along with its context and consequences. They serve as a historical record for understanding why certain decisions were made.

## ADR Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [001](001-aspire-orchestration.md) | Use .NET Aspire for Orchestration | Accepted | 2026-01-08 |
| [002](002-opentelemetry-over-appinsights.md) | OpenTelemetry over Application Insights SDK | Accepted | 2026-01-08 |
| [003](003-blazor-wasm-architecture.md) | Blazor WebAssembly with API Host | Accepted | 2026-01-08 |
| [004](004-vertical-slice-features.md) | Vertical Slice Feature Organization | Accepted | 2026-01-08 |

## Template

When adding new ADRs, use the following template:

```markdown
# ADR-XXX: Title

## Status
Proposed | Accepted | Deprecated | Superseded

## Context
What is the issue that we're seeing that is motivating this decision?

## Decision
What is the change that we're proposing and/or doing?

## Consequences
What becomes easier or more difficult because of this change?
```
