using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Po.TicTac.Api.Services;
using Po.TicTac.Shared.DTOs;

namespace Po.TicTac.Api.Features.Players;

/// <summary>
/// Query to retrieve a single player's statistics.
/// </summary>
public sealed record GetPlayerStatsQuery(string PlayerName) : IRequest<PlayerStatsDto?>;

/// <summary>
/// Handler for <see cref="GetPlayerStatsQuery"/>.
/// </summary>
public sealed class GetPlayerStatsHandler : IRequestHandler<GetPlayerStatsQuery, PlayerStatsDto?>
{
    private readonly StorageService _storageService;
    private readonly ILogger<GetPlayerStatsHandler> _logger;

    public GetPlayerStatsHandler(StorageService storageService, ILogger<GetPlayerStatsHandler> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<PlayerStatsDto?> Handle(GetPlayerStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving stats for player: {PlayerName}", request.PlayerName);

        var stats = await _storageService.GetPlayerStatsAsync(request.PlayerName);
        if (stats is null)
        {
            _logger.LogInformation("No stats found for player: {PlayerName}", request.PlayerName);
            return null;
        }

        return new PlayerStatsDto
        {
            Name = request.PlayerName,
            Stats = stats
        };
    }
}

/// <summary>
/// Endpoint configuration for retrieving player statistics.
/// </summary>
public static class GetPlayerStatsEndpoint
{
    public static IEndpointRouteBuilder MapGetPlayerStats(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/players/{playerName}/stats", async (string playerName, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPlayerStatsQuery(playerName));
            // Return empty stats for new players instead of 404
            if (result is null)
            {
                return Results.Ok(new PlayerStatsDto
                {
                    Name = playerName,
                    Stats = new Po.TicTac.Shared.Models.PlayerStats()
                });
            }
            return Results.Ok(result);
        })
        .WithName("GetPlayerStats")
        .WithTags("Players")
        .WithSummary("Retrieves statistics for a specific player")
        .WithDescription("Returns the stored statistics for the requested player, or empty stats for new players.")
        .Produces<PlayerStatsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
