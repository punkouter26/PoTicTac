using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Po.TicTac.Api.Services;
using Po.TicTac.Api.DTOs;
using Po.TicTac.Api.Models;

namespace Po.TicTac.Api.Features.Players;

/// <summary>
/// Query to retrieve a single player's statistics.
/// </summary>
public sealed record GetPlayerStatsQuery(string PlayerName) : IRequest<PlayerStatsDto?>;

/// <summary>
/// Handler for <see cref="GetPlayerStatsQuery"/> using primary constructor (C# 14).
/// </summary>
public sealed class GetPlayerStatsHandler(
    StorageService storageService,
    ILogger<GetPlayerStatsHandler> logger) : IRequestHandler<GetPlayerStatsQuery, PlayerStatsDto?>
{
    public async Task<PlayerStatsDto?> Handle(GetPlayerStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving stats for player: {PlayerName}", request.PlayerName);

        var stats = await storageService.GetPlayerStatsAsync(request.PlayerName);
        if (stats is null)
        {
            logger.LogInformation("No stats found for player: {PlayerName}", request.PlayerName);
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
                    Stats = new PlayerStats()
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
