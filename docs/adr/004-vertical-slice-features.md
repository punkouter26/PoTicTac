# ADR-004: Vertical Slice Feature Organization

## Status
Accepted

## Date
2026-01-08

## Context

Traditional layered architecture organizes code by technical concern:
```
Controllers/
Services/
Repositories/
Models/
```

This leads to:
- Shotgun surgery when adding features
- Difficult to understand feature scope
- Tight coupling between layers

## Decision

Organize API code by **vertical slices** using MediatR CQRS pattern:

```
Features/
├── Health/
│   └── HealthCheckEndpoint.cs
├── Players/
│   ├── GetPlayerStats.cs      # Query + Handler + Endpoint
│   └── SavePlayerStats.cs     # Command + Handler + Endpoint
└── Statistics/
    ├── GetAllPlayerStatistics.cs
    └── GetLeaderboard.cs
```

Each feature file contains:
1. **Request/Query/Command** - The input DTO
2. **Handler** - The business logic
3. **Endpoint** - The Minimal API mapping

```csharp
// GetLeaderboard.cs - Single file, complete feature

// 1. Query
public record GetLeaderboardQuery(int Limit) : IRequest<IEnumerable<PlayerStatsDto>>;

// 2. Handler
public class GetLeaderboardHandler : IRequestHandler<GetLeaderboardQuery, IEnumerable<PlayerStatsDto>>
{
    public async Task<IEnumerable<PlayerStatsDto>> Handle(GetLeaderboardQuery request, ...)
    {
        // Business logic here
    }
}

// 3. Endpoint
public static class GetLeaderboardEndpoint
{
    public static IEndpointRouteBuilder MapGetLeaderboard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/statistics/leaderboard", async (IMediator mediator, int limit = 10) =>
        {
            var result = await mediator.Send(new GetLeaderboardQuery(limit));
            return Results.Ok(result);
        });
        return app;
    }
}
```

## Consequences

### Positive
- **Feature cohesion**: All code for a feature in one file
- **Easy navigation**: Find feature by name, not by layer
- **Independent changes**: Modify one feature without touching others
- **Clear boundaries**: Each feature is a mini-module
- **Testable**: Handler can be unit tested in isolation

### Negative
- **Boilerplate**: MediatR adds Request/Handler ceremony
- **Learning curve**: Developers must understand CQRS concepts
- **Potential duplication**: Some utilities may be repeated across features

### Guidelines
- Keep features small (< 150 lines per file)
- Shared logic goes in `Services/` folder
- Cross-cutting concerns handled by MediatR behaviors
