using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.Models;

namespace Po.TicTac.Api.Features.Players;

/// <summary>
/// Command to save or update player statistics
/// </summary>
public record SavePlayerStatsCommand(string PlayerName, PlayerStats Stats) : IRequest<Unit>;

/// <summary>
/// Handler for SavePlayerStatsCommand using primary constructor (C# 14).
/// Issue 9 fix: Invalidates leaderboard cache on stats update.
/// </summary>
public sealed class SavePlayerStatsHandler(
    StorageService storageService,
    HybridCache cache,
    ILogger<SavePlayerStatsHandler> logger) : IRequestHandler<SavePlayerStatsCommand, Unit>
{
    public async Task<Unit> Handle(SavePlayerStatsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Saving stats for player: {PlayerName}", request.PlayerName);

        try
        {
            await storageService.SavePlayerStatsAsync(request.PlayerName, request.Stats);
            
            // Issue 9 fix: Invalidate leaderboard cache to reflect updated stats
            await cache.RemoveAsync("leaderboard:10", cancellationToken);
            logger.LogDebug("Invalidated leaderboard cache after stats update");
            
            logger.LogInformation("Successfully saved stats for player: {PlayerName}", request.PlayerName);
            return Unit.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving stats for player: {PlayerName}", request.PlayerName);
            throw;
        }
    }
}

/// <summary>
/// Endpoint configuration for SavePlayerStats
/// </summary>
public static class SavePlayerStatsEndpoint
{
    public static IEndpointRouteBuilder MapSavePlayerStats(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/players/{playerName}/stats", async (string playerName, PlayerStats stats, IMediator mediator) =>
        {
            await mediator.Send(new SavePlayerStatsCommand(playerName, stats));
            return Results.NoContent();
        })
        .WithName("SavePlayerStats")
        .WithTags("Players")
        .WithSummary("Saves or updates statistics for a specific player")
        .WithDescription("Stores player statistics including wins, losses, draws, win rate, streaks, and other metrics broken down by difficulty level.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
