using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Po.TicTac.Api.Telemetry;

/// <summary>
/// OpenTelemetry instrumentation for PoTicTac application
/// </summary>
public class PoTicTacTelemetryV2
{
    public const string ServiceName = "PoTicTac";
    public const string ServiceVersion = "1.0.0";

    // ActivitySource for distributed tracing
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    // Meter for custom metrics
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    // Custom metrics
    public static readonly Counter<long> GamesPlayedCounter = Meter.CreateCounter<long>(
        "potictac.games.played",
        description: "Total number of games played");

    public static readonly Counter<long> GamesWonCounter = Meter.CreateCounter<long>(
        "potictac.games.won",
        description: "Total number of games won");

    public static readonly Counter<long> GamesDrawCounter = Meter.CreateCounter<long>(
        "potictac.games.draw",
        description: "Total number of games ending in a draw");

    public static readonly Histogram<double> GameDurationHistogram = Meter.CreateHistogram<double>(
        "potictac.game.duration",
        unit: "s",
        description: "Duration of games in seconds");

    public static readonly Histogram<int> MovesPerGameHistogram = Meter.CreateHistogram<int>(
        "potictac.game.moves",
        description: "Number of moves per game");

    public static readonly Counter<long> ApiRequestsCounter = Meter.CreateCounter<long>(
        "potictac.api.requests",
        description: "Total number of API requests");

    public static readonly Histogram<double> ApiRequestDurationHistogram = Meter.CreateHistogram<double>(
        "potictac.api.request.duration",
        unit: "ms",
        description: "Duration of API requests in milliseconds");

    public static readonly Counter<long> SignalRConnectionsCounter = Meter.CreateCounter<long>(
        "potictac.signalr.connections",
        description: "Total SignalR connections");

    public static readonly UpDownCounter<int> ActiveGamesGauge = Meter.CreateUpDownCounter<int>(
        "potictac.games.active",
        description: "Number of currently active games");

    /// <summary>
    /// Record a game played event
    /// </summary>
    public static void RecordGamePlayed(string gameMode, string difficulty, int moves, double durationSeconds, string winner)
    {
        using var activity = ActivitySource.StartActivity("GamePlayed");
        activity?.SetTag("game.mode", gameMode);
        activity?.SetTag("game.difficulty", difficulty);
        activity?.SetTag("game.moves", moves);
        activity?.SetTag("game.duration", durationSeconds);
        activity?.SetTag("game.winner", winner);

        GamesPlayedCounter.Add(1, new KeyValuePair<string, object?>("game.mode", gameMode));

        if (winner != "Draw")
        {
            GamesWonCounter.Add(1, new KeyValuePair<string, object?>("winner", winner));
        }
        else
        {
            GamesDrawCounter.Add(1);
        }

        GameDurationHistogram.Record(durationSeconds);
        MovesPerGameHistogram.Record(moves);
    }

    /// <summary>
    /// Record an API request
    /// </summary>
    public static Activity? StartApiRequestActivity(string endpoint, string method)
    {
        var activity = ActivitySource.StartActivity($"{method} {endpoint}");
        activity?.SetTag("http.method", method);
        activity?.SetTag("http.endpoint", endpoint);
        return activity;
    }

    /// <summary>
    /// Record API request completion
    /// </summary>
    public static void RecordApiRequest(string endpoint, string method, double durationMs, int statusCode)
    {
        ApiRequestsCounter.Add(1,
            new KeyValuePair<string, object?>("http.endpoint", endpoint),
            new KeyValuePair<string, object?>("http.method", method),
            new KeyValuePair<string, object?>("http.status_code", statusCode));

        ApiRequestDurationHistogram.Record(durationMs,
            new KeyValuePair<string, object?>("http.endpoint", endpoint),
            new KeyValuePair<string, object?>("http.status_code", statusCode));
    }

    /// <summary>
    /// Record SignalR connection event
    /// </summary>
    public static void RecordSignalRConnection(string connectionType)
    {
        using var activity = ActivitySource.StartActivity("SignalRConnection");
        activity?.SetTag("connection.type", connectionType);

        SignalRConnectionsCounter.Add(1, new KeyValuePair<string, object?>("connection.type", connectionType));
    }

    /// <summary>
    /// Update active games count
    /// </summary>
    public static void UpdateActiveGames(int delta)
    {
        ActiveGamesGauge.Add(delta);
    }
}
