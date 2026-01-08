using MediatR;
using Po.TicTac.Api.Services;
using Po.TicTac.Shared.Models;

namespace Po.TicTac.Api.Features.Players;

/// <summary>
/// Command to save or update player statistics
/// </summary>
public record SavePlayerStatsCommand(string PlayerName, PlayerStats Stats) : IRequest<Unit>;

/// <summary>
/// Handler for SavePlayerStatsCommand
/// </summary>
public class SavePlayerStatsHandler : IRequestHandler<SavePlayerStatsCommand, Unit>
{
    private readonly StorageService _storageService;
    private readonly ILogger<SavePlayerStatsHandler> _logger;

    public SavePlayerStatsHandler(StorageService storageService, ILogger<SavePlayerStatsHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Unit> Handle(SavePlayerStatsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving stats for player: {PlayerName}", request.PlayerName);

        try
        {
            await _storageService.SavePlayerStatsAsync(request.PlayerName, request.Stats);
            _logger.LogInformation("Successfully saved stats for player: {PlayerName}", request.PlayerName);
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving stats for player: {PlayerName}", request.PlayerName);
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
