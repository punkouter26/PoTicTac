using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PoTicTacServer.Telemetry;

/// <summary>
/// Centralized telemetry provider for custom traces and metrics
/// </summary>
public static class PoTicTacTelemetry
{
    /// <summary>
    /// Activity source for distributed tracing of game operations
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("PoTicTac", "1.0.0");

    /// <summary>
    /// Meter for custom business metrics
    /// </summary>
    public static readonly Meter Meter = new("PoTicTac", "1.0.0");

    // Game-related counters
    public static readonly Counter<long> GamesStarted = Meter.CreateCounter<long>(
        "potictac.games.started",
        description: "Total number of games started");

    public static readonly Counter<long> GamesCompleted = Meter.CreateCounter<long>(
        "potictac.games.completed",
        description: "Total number of games completed");

    public static readonly Counter<long> MovesPlayed = Meter.CreateCounter<long>(
        "potictac.moves.played",
        description: "Total number of moves played");

    public static readonly Histogram<double> GameDuration = Meter.CreateHistogram<double>(
        "potictac.game.duration",
        unit: "seconds",
        description: "Duration of completed games in seconds");

    public static readonly Histogram<double> AiCalculationTime = Meter.CreateHistogram<double>(
        "potictac.ai.calculation_time",
        unit: "milliseconds",
        description: "Time taken for AI to calculate next move");

    public static readonly Histogram<int> MovesPerGame = Meter.CreateHistogram<int>(
        "potictac.moves.per_game",
        description: "Number of moves per completed game");

    public static readonly Counter<long> PlayerWins = Meter.CreateCounter<long>(
        "potictac.wins.player",
        description: "Total wins by human players");

    public static readonly Counter<long> AiWins = Meter.CreateCounter<long>(
        "potictac.wins.ai",
        description: "Total wins by AI");

    public static readonly Counter<long> Draws = Meter.CreateCounter<long>(
        "potictac.games.draws",
        description: "Total number of draw games");

    // Multiplayer-specific metrics
    public static readonly Counter<long> MultiplayerGamesStarted = Meter.CreateCounter<long>(
        "potictac.multiplayer.games.started",
        description: "Total multiplayer games started");

    public static readonly Histogram<int> PlayersConnected = Meter.CreateHistogram<int>(
        "potictac.multiplayer.players.connected",
        description: "Number of players connected concurrently");

    // Performance metrics
    public static readonly Histogram<double> ApiResponseTime = Meter.CreateHistogram<double>(
        "potictac.api.response_time",
        unit: "milliseconds",
        description: "API endpoint response time");

    public static readonly Histogram<long> StorageOperationTime = Meter.CreateHistogram<long>(
        "potictac.storage.operation_time",
        unit: "milliseconds",
        description: "Azure Table Storage operation duration");

    // Error metrics
    public static readonly Counter<long> GameErrors = Meter.CreateCounter<long>(
        "potictac.errors.game",
        description: "Total game-related errors");

    public static readonly Counter<long> StorageErrors = Meter.CreateCounter<long>(
        "potictac.errors.storage",
        description: "Total storage operation errors");
}
