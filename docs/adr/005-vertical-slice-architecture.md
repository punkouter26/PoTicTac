# ADR-005: Adopt Vertical Slice Architecture

## Status
**Accepted** - August 2, 2025

## Context

We needed to choose an architectural pattern for organizing the PoTicTac codebase. The requirements were:

- **Simplicity**: Easy to understand for developers new to the project
- **Feature Cohesion**: Related code should be located together
- **Minimal Coupling**: Reduce dependencies between different features
- **Rapid Development**: Enable fast feature delivery without extensive refactoring
- **Testability**: Easy to write focused unit and integration tests
- **Small Team**: Pattern suitable for 1-3 developers (not large enterprise teams)
- **Low Ceremony**: Avoid excessive abstractions and boilerplate code

Traditional layered architectures (e.g., Controller → Service → Repository) spread feature logic across multiple folders, making it harder to understand complete feature workflows.

## Decision

We will adopt **Vertical Slice Architecture** as the primary organizational pattern, with **Clean Architecture principles** applied where complexity demands it.

### Core Principles

1. **Organize by Feature**: Group code by use case/feature, not by technical layer
2. **Self-Contained Slices**: Each feature contains its own models, logic, and data access
3. **Minimal Shared Abstractions**: Share only when duplication becomes painful
4. **SOLID When Needed**: Apply principles pragmatically, not dogmatically
5. **Refactor Ruthlessly**: Keep files ≤500 lines through proactive refactoring

### Current Structure

```
PoTicTacServer/
├── Controllers/
│   ├── HealthController.cs        # Health monitoring feature slice
│   ├── PlayersController.cs       # Player management feature slice
│   └── StatisticsController.cs    # Statistics/leaderboard feature slice
├── Hubs/
│   └── GameHub.cs                 # Real-time multiplayer feature slice
├── Services/
│   └── StorageService.cs          # Shared storage abstraction
├── Models/
│   └── PlayerStats.cs             # Shared domain models
└── HealthChecks/
    └── StorageHealthCheck.cs      # Health check implementation
```

### Feature Slice Example: Statistics

```
StatisticsController.cs (65 lines)
├── GetAllPlayerStatistics()     → StorageService.GetAllPlayersAsync()
├── GetLeaderboard()              → StorageService.GetLeaderboardAsync()
└── CreateTestData()              → StorageService.SavePlayerStatsAsync()

StorageService.cs (120 lines)
├── GetPlayerStatsAsync()
├── SavePlayerStatsAsync()
├── GetAllPlayersAsync()
└── GetLeaderboardAsync()

PlayerStats.cs (15 lines)
└── Data model shared across features
```

**Benefits of This Slice**:
- All statistics logic in one controller
- Clear dependencies (Controller → StorageService → Azure Table Storage)
- Easy to test (mock StorageService)
- Feature can be understood without navigating multiple folders

## Consequences

### Positive

✅ **Feature Locality**: All code for a feature is in one place (easy to find)  
✅ **Faster Development**: Add new features without touching existing slices  
✅ **Easier Onboarding**: New developers can understand one slice at a time  
✅ **Reduced Coupling**: Features don't share logic unless necessary  
✅ **Simpler Tests**: Test one vertical slice without complex setup  
✅ **Clear Boundaries**: Easy to see dependencies between slices  
✅ **No Over-Engineering**: Avoid creating abstractions before they're needed  

### Negative

⚠️ **Potential Duplication**: Some code may be duplicated across slices (acceptable trade-off)  
⚠️ **Unclear Shared Logic**: Less obvious where to put cross-cutting concerns (solved with /Services folder)  
⚠️ **Discipline Required**: Developers must resist urge to create premature abstractions  
⚠️ **Not Traditional**: May confuse developers trained only in layered architecture  

### Trade-offs

- **DRY (Don't Repeat Yourself) vs. Decoupling**: Prefer decoupling; tolerate small duplication
- **Layering vs. Cohesion**: Sacrifice perfect layering for feature cohesion
- **Abstraction vs. Simplicity**: Create abstractions only when pain justifies it

## Alternatives Considered

### 1. Clean Architecture (Onion/Hexagonal)
**Pros**: Clear dependency rules, highly testable, DDD-friendly, enterprise-grade  
**Cons**: **Over-engineered for small projects**, many folders/projects (Domain, Application, Infrastructure), excessive interfaces, slow feature delivery  
**Why Rejected**: Too much ceremony for a game application with simple CRUD operations; would require 4-5 projects instead of 2

**Example Complexity**:
```
PoTicTac.Domain/          (Core entities, no dependencies)
PoTicTac.Application/     (Use cases, interfaces)
PoTicTac.Infrastructure/  (Data access implementations)
PoTicTac.Api/             (Controllers, thin layer)
PoTicTac.Shared/          (DTOs)
```

This is overkill for our use case.

### 2. Layered Architecture (N-Tier)
**Pros**: Familiar, well-understood, clear separation of concerns, extensive documentation  
**Cons**: **Feature logic scattered** across layers, more files to navigate, encourages anemic domain models, slower feature delivery  
**Why Rejected**: Forces navigation through multiple folders to understand one feature; our features are simple and don't benefit from rigid layering

**Example**:
```
Controllers/PlayersController.cs       # HTTP layer
Services/PlayerService.cs              # Business logic layer
Repositories/PlayerRepository.cs       # Data access layer
Models/Player.cs                       # Domain layer
```

To understand "get player stats", you must open 4 files across 4 folders.

### 3. Feature Folders (MVC)
**Pros**: Groups by feature like Vertical Slice, simple folder structure  
**Cons**: **MVC-specific pattern**, doesn't work well with ASP.NET Core Web API, less opinionated about dependencies  
**Why Rejected**: Similar to Vertical Slice but less opinionated; we prefer the explicit slice pattern

### 4. Domain-Driven Design (DDD)
**Pros**: Rich domain models, bounded contexts, ubiquitous language, great for complex domains  
**Cons**: **Massive over-engineering** for simple CRUD, requires domain experts, slow initial development, steep learning curve  
**Why Rejected**: Our domain (Tic Tac Toe game) is trivial; DDD's strategic patterns (Aggregates, Entities, Value Objects) add no value

### 5. Modular Monolith
**Pros**: Clear module boundaries, can extract to microservices later, supports large teams  
**Cons**: Requires strict module contracts, more complex than needed, premature optimization for future microservices  
**Why Rejected**: We don't have multiple teams or plans to split into microservices; simpler patterns suffice

## Implementation Guidelines

### When to Create a New Slice
Create a new vertical slice when:
- Implementing a new user-facing feature
- Adding a new API endpoint or hub method
- The feature has distinct business logic

### When to Extract Shared Code
Extract to `/Services` when:
- Same code is duplicated in 3+ slices
- Logic is infrastructure concern (e.g., StorageService, EmailService)
- Feature is truly cross-cutting (e.g., authentication, logging)

**Current Shared Services**:
- **StorageService**: Azure Table Storage abstraction (used by 3 controllers)
- **CorrelationEnricher**: Logging enricher (used by all requests)
- **CustomTelemetryInitializer**: Application Insights telemetry (used by all requests)

### File Size Enforcement

**Rule**: All files must be ≤500 lines

**Enforcement**:
1. Code reviews fail PRs with >500 line files
2. Proactive refactoring when approaching 400 lines
3. Extract methods, classes, or services as needed

**Example Refactoring**:
```
GameHub.cs (550 lines - TOO BIG)

Refactor to:
GameHub.cs (300 lines)          # Hub methods only
GameState.cs (100 lines)         # Game state management
GameSessionManager.cs (150 lines) # Session lifecycle
```

### Dependency Flow

```
┌─────────────────┐
│ Controllers/Hubs│  ← HTTP/SignalR entry points
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Services     │  ← Shared business logic
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Azure Services │  ← External dependencies
└─────────────────┘
```

**Rules**:
- Controllers/Hubs depend on Services
- Services depend on Azure SDK
- No circular dependencies
- Models can be shared freely

## SOLID Principles Application

We apply SOLID principles **pragmatically**, not dogmatically:

### Single Responsibility Principle (SRP)
✅ **Applied**: Each controller handles one feature area  
✅ **Applied**: StorageService only handles data persistence  
❌ **Not Applied**: We don't create separate classes for every tiny behavior

### Open/Closed Principle (OCP)
✅ **Applied**: AI strategies use interface (`IAIStrategy`) for extensibility  
❌ **Not Applied**: We don't preemptively create extension points "just in case"

### Liskov Substitution Principle (LSP)
✅ **Applied**: All `IAIStrategy` implementations are substitutable  
✅ **Applied**: Proper inheritance hierarchies

### Interface Segregation Principle (ISP)
⚠️ **Partially Applied**: We create interfaces when multiple implementations exist (e.g., `IAIStrategy`)  
❌ **Not Applied**: We don't create interfaces for single implementations (YAGNI)

### Dependency Inversion Principle (DIP)
✅ **Applied**: Controllers depend on `StorageService` abstraction, not `TableClient` directly  
⚠️ **Partially Applied**: `StorageService` is a concrete class, not an interface (acceptable for single implementation)

## Testing Approach

### Unit Tests
- Test individual methods in isolation
- Mock external dependencies (StorageService)
- Focus on business logic (e.g., GameLogicService, HardAIStrategy)

### Integration Tests
- Test entire vertical slice (Controller → StorageService → Azurite)
- Use WebApplicationFactory
- Verify HTTP contracts

### E2E Tests
- Test user workflows across multiple slices
- Use Playwright
- Simulate real browser interactions

## Refactoring Triggers

Refactor when:
1. File exceeds 400 lines (approaching limit)
2. Method exceeds 50 lines
3. Code duplicated in 3+ places
4. Feature becomes too complex to understand in <5 minutes
5. Tests become difficult to write

## References

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/) by Jimmy Bogard
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Avoiding Over-Engineering](https://martinfowler.com/bliki/Yagni.html) by Martin Fowler

## Review Date
**Next Review**: February 2026 (6 months)  
**Review Trigger**: If codebase exceeds 10,000 lines or team grows beyond 5 developers

## Related ADRs
- [ADR-001: Use Blazor WebAssembly for Client Application](./001-blazor-webassembly.md)
- [ADR-007: Implement Minimax Algorithm for Hard AI](./007-minimax-ai-strategy.md)
