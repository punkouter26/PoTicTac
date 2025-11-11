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
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPlayerStats")
        .WithTags("Players")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Retrieves statistics for a specific player",
            Description = "Returns the stored statistics for the requested player, or 404 if not found."
        })
        .Produces<PlayerStatsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
