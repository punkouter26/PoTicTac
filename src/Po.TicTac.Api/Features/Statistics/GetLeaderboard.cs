using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.DTOs;

namespace Po.TicTac.Api.Features.Statistics;

/// <summary>
/// Query to retrieve leaderboard
/// </summary>
public record GetLeaderboardQuery(int Limit = 10) : IRequest<IEnumerable<PlayerStatsDto>>;

/// <summary>
/// Handler for GetLeaderboardQuery using primary constructor (C# 14) with HybridCache.
/// </summary>
public sealed class GetLeaderboardHandler(
    StorageService storageService,
    HybridCache cache,
    ILogger<GetLeaderboardHandler> logger) : IRequestHandler<GetLeaderboardQuery, IEnumerable<PlayerStatsDto>>
{
    public async Task<IEnumerable<PlayerStatsDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving leaderboard with limit: {Limit}", request.Limit);

        try
        {
            var cacheKey = $"leaderboard:{request.Limit}";
            var playerStatsDtos = await cache.GetOrCreateAsync(
                cacheKey,
                async ct =>
                {
                    logger.LogDebug("Cache miss for leaderboard, fetching from storage");
                    var leaderboard = await storageService.GetLeaderboardAsync(request.Limit);
                    return leaderboard.Select(p => new PlayerStatsDto { Name = p.Name, Stats = p.Stats }).ToList();
                },
                cancellationToken: cancellationToken);

            logger.LogInformation("Successfully retrieved {Count} players for the leaderboard", playerStatsDtos?.Count ?? 0);
            return playerStatsDtos ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving leaderboard");
            throw;
        }
    }
}

/// <summary>
/// Endpoint configuration for GetLeaderboard
/// </summary>
public static class GetLeaderboardEndpoint
{
    public static IEndpointRouteBuilder MapGetLeaderboard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/statistics/leaderboard", async (IMediator mediator, int limit = 10) =>
        {
            var result = await mediator.Send(new GetLeaderboardQuery(limit));
            return Results.Ok(result);
        })
        .WithName("GetLeaderboard")
        .WithTags("Statistics")
        .WithSummary("Retrieves the top players based on win rate")
        .WithDescription("Returns players ranked by win rate in descending order. Useful for displaying leaderboards and competitive rankings.")
        .Produces<IEnumerable<PlayerStatsDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
