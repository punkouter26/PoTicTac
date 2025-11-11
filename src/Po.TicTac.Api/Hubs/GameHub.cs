using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Po.TicTac.Api.Services;
using Po.TicTac.Shared.Models;

namespace Po.TicTac.Api.Hubs;

// Game state representation
public class GameState
{
    public required string GameId { get; set; }
    public required List<List<int>> Board { get; set; }
    public int CurrentPlayer { get; set; }
    public required string GameStatus { get; set; }
    public int? Winner { get; set; }
    public required List<Player> Players { get; set; }
    public required List<Move> MoveHistory { get; set; }
    public required List<int[]> WinningCells { get; set; }
}

public class Player
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public int Symbol { get; set; }
    public required PlayerStats Stats { get; set; }
}

public class Move
{
    public int Player { get; set; }
    public required int[] Position { get; set; }
    public long Timestamp { get; set; }
}

// Game logic for server-side validation
public class GameLogic
{
    private const int BOARD_SIZE = 6;
    private const int WIN_LENGTH = 4;

    public static List<List<int>> CreateEmptyBoard()
    {
        return Enumerable.Range(0, BOARD_SIZE)
            .Select(_ => Enumerable.Repeat(0, BOARD_SIZE).ToList())
            .ToList();
    }

    public static GameState CreateInitialState(string gameId, Player player1)
    {
        var board = CreateEmptyBoard();
        var players = new List<Player> { player1 };

        return new GameState
        {
            GameId = gameId,
            Board = board,
            CurrentPlayer = 1,
            GameStatus = "waiting", // waiting for second player
            Winner = null,
            Players = players,
            MoveHistory = new List<Move>(),
            WinningCells = new List<int[]>()
        };
    }

    public static bool IsValidMove(GameState state, int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE)
        {
            return false;
        }

        if (state.Board[row][col] != 0)
        {
            return false;
        }

        if (state.GameStatus != "playing")
        {
            return false;
        }

        return true;
    }

    public static GameState MakeMove(GameState state, int row, int col)
    {
        if (!IsValidMove(state, row, col))
        {
            return state;
        }

        var newBoard = state.Board.Select(r => r.ToList()).ToList();
        newBoard[row][col] = state.CurrentPlayer;

        var move = new Move
        {
            Player = state.CurrentPlayer,
            Position = new[] { row, col },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var newMoveHistory = state.MoveHistory.ToList();
        newMoveHistory.Add(move);

        // Check for win
        var winningCells = CheckWinner(newBoard, row, col);
        if (winningCells != null)
        {
            return new GameState
            {
                GameId = state.GameId,
                Board = newBoard,
                CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
                GameStatus = "won",
                Winner = state.CurrentPlayer,
                Players = state.Players,
                MoveHistory = newMoveHistory,
                WinningCells = winningCells
            };
        }

        // Check for draw
        if (IsDraw(newBoard))
        {
            return new GameState
            {
                GameId = state.GameId,
                Board = newBoard,
                CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
                GameStatus = "draw",
                Winner = null,
                Players = state.Players,
                MoveHistory = newMoveHistory,
                WinningCells = new List<int[]>()
            };
        }

        // Continue game
        return new GameState
        {
            GameId = state.GameId,
            Board = newBoard,
            CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
            GameStatus = "playing",
            Winner = null,
            Players = state.Players,
            MoveHistory = newMoveHistory,
            WinningCells = new List<int[]>()
        };
    }

    /// <summary>
    /// Lightweight win checker for server-side validation.
    /// Checks all 4 directions from the last move position.
    /// Refactored to reduce complexity and eliminate duplication.
    /// </summary>
    private static List<int[]>? CheckWinner(List<List<int>> board, int row, int col)
    {
        var player = board[row][col];
        (int dx, int dy)[] directions = [(0, 1), (1, 0), (1, 1), (1, -1)];

        foreach (var (dx, dy) in directions)
        {
            var line = CountLine(board, row, col, dx, dy, player);
            if (line.Count >= WIN_LENGTH)
            {
                return line;
            }
        }

        return null;
    }

    /// <summary>
    /// Counts consecutive pieces in both directions from a position.
    /// </summary>
    private static List<int[]> CountLine(List<List<int>> board, int row, int col, int dx, int dy, int player)
    {
        var line = new List<int[]> { new[] { row, col } };

        // Check positive direction
        for (int i = 1; i < WIN_LENGTH; i++)
        {
            int newRow = row + dx * i;
            int newCol = col + dy * i;
            if (!IsValidPosition(newRow, newCol) || board[newRow][newCol] != player)
            {
                break;
            }
            line.Add(new[] { newRow, newCol });
        }

        // Check negative direction
        for (int i = 1; i < WIN_LENGTH; i++)
        {
            int newRow = row - dx * i;
            int newCol = col - dy * i;
            if (!IsValidPosition(newRow, newCol) || board[newRow][newCol] != player)
            {
                break;
            }
            line.Add(new[] { newRow, newCol });
        }

        return line;
    }

    private static bool IsValidPosition(int row, int col) =>
        row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;

    private static bool IsDraw(List<List<int>> board)
    {
        return board.All(row => row.All(cell => cell != 0));
    }
}

public class GameHub : Hub
{
    private static readonly ConcurrentDictionary<string, GameState> _games = new();
    private static readonly ConcurrentDictionary<string, string> _userGameMap = new();
    private readonly StorageService _storageService;

    public GameHub(StorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<string> CreateGame(string playerName)
    {
        var gameId = GenerateGameId();
        var playerId = Context.ConnectionId;

        var player = new Player
        {
            Id = playerId,
            Name = playerName,
            Type = "human",
            Symbol = 1,
            Stats = new PlayerStats() // Empty stats for multiplayer (stats tracking disabled for now)
        };

        var gameState = GameLogic.CreateInitialState(gameId, player);
        _games[gameId] = gameState;
        _userGameMap[playerId] = gameId;

        await Groups.AddToGroupAsync(playerId, gameId);
        await Clients.Caller.SendAsync("GameCreated", gameId);

        return gameId;
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new HubException("Game not found");
        }

        if (gameState.Players.Count >= 2)
        {
            throw new HubException("Game is already full");
        }

        var playerId = Context.ConnectionId;
        var player = new Player
        {
            Id = playerId,
            Name = playerName,
            Type = "human",
            Symbol = 2,
            Stats = new PlayerStats() // Empty stats for multiplayer (stats tracking disabled for now)
        };

        gameState.Players.Add(player);
        gameState.GameStatus = "playing";
        _userGameMap[playerId] = gameId;

        await Groups.AddToGroupAsync(playerId, gameId);

        // Notify existing player about the new player
        await Clients.Group(gameId).SendAsync("PlayerJoined", player);

        // Send the complete game state to both players
        await Clients.Group(gameId).SendAsync("GameJoined", gameState, gameState.Players);
    }

    public async Task MakeMove(string gameId, int row, int col)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new HubException("Game not found");
        }

        var playerId = Context.ConnectionId;
        var player = gameState.Players.FirstOrDefault(p => p.Id == playerId) ?? throw new HubException("Player not found in this game");

        if (player.Symbol != gameState.CurrentPlayer)
        {
            throw new HubException("It's not your turn");
        }

        if (!GameLogic.IsValidMove(gameState, row, col))
        {
            throw new HubException("Invalid move");
        }

        var newGameState = GameLogic.MakeMove(gameState, row, col);
        _games[gameId] = newGameState;

        var lastMove = newGameState.MoveHistory.LastOrDefault();
        await Clients.Group(gameId).SendAsync("MoveReceived", newGameState, lastMove);

        if (newGameState.GameStatus != "playing")
        {
            // TODO: Re-enable multiplayer stats with new per-difficulty schema
            // await SaveGameStatistics(newGameState);
            await Clients.Group(gameId).SendAsync("GameOver", newGameState);
        }
    }

    public async Task RequestRematch(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new HubException("Game not found");
        }

        var playerId = Context.ConnectionId;
        await Clients.OthersInGroup(gameId).SendAsync("RematchRequested", playerId);
    }

    public async Task AcceptRematch(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var gameState))
        {
            throw new HubException("Game not found");
        }

        // Reset the game state for a rematch
        var newGameState = new GameState
        {
            GameId = gameId,
            Board = GameLogic.CreateEmptyBoard(),
            CurrentPlayer = 1,
            GameStatus = "playing",
            Winner = null,
            Players = gameState.Players,
            MoveHistory = new List<Move>(),
            WinningCells = new List<int[]>()
        };

        _games[gameId] = newGameState;
        await Clients.Group(gameId).SendAsync("RematchAccepted", newGameState);
    }

    public async Task LeaveGame(string gameId)
    {
        var playerId = Context.ConnectionId;

        if (_userGameMap.TryRemove(playerId, out _))
        {
            await Groups.RemoveFromGroupAsync(playerId, gameId);
            await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", playerId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = Context.ConnectionId;

        if (_userGameMap.TryRemove(playerId, out var gameId))
        {
            await Groups.RemoveFromGroupAsync(playerId, gameId);
            await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", playerId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string GenerateGameId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    /* TODO: Re-enable multiplayer stats with new per-difficulty schema
    private async Task SaveGameStatistics(GameState gameState)
    {
        // Only save stats for human players
        var humanPlayers = gameState.Players.Where(p => p.Type == "human").ToList();

        foreach (var player in humanPlayers)
        {
            // Get existing stats or create new
            var existingStats = await _storageService.GetPlayerStatsAsync(player.Name);

            PlayerStats updatedStats;
            if (existingStats != null)
            {
                updatedStats = existingStats;
            }
            else
            {
                updatedStats = new PlayerStats
                {
                    Wins = 0,
                    Losses = 0,
                    Draws = 0,
                    TotalGames = 0,
                    WinStreak = 0,
                    CurrentStreak = 0,
                    AverageMovesPerGame = 0,
                    TotalMoves = 0,
                    WinRate = 0
                };
            }

            // Update stats based on game outcome
            updatedStats.TotalGames++;

            if (gameState.GameStatus == "won")
            {
                if (gameState.Winner == player.Symbol)
                {
                    // Player won
                    updatedStats.Wins++;
                    updatedStats.CurrentStreak++;
                    if (updatedStats.CurrentStreak > updatedStats.WinStreak)
                    {
                        updatedStats.WinStreak = updatedStats.CurrentStreak;
                    }
                }
                else
                {
                    // Player lost
                    updatedStats.Losses++;
                    updatedStats.CurrentStreak = 0;
                }
            }
            else if (gameState.GameStatus == "draw")
            {
                updatedStats.Draws++;
                updatedStats.CurrentStreak = 0;
            }

            // Update total moves and average
            var playerMoves = gameState.MoveHistory.Count(m => m.Player == player.Symbol);
            updatedStats.TotalMoves += playerMoves;
            updatedStats.AverageMovesPerGame = updatedStats.TotalGames > 0 
                ? (double)updatedStats.TotalMoves / updatedStats.TotalGames 
                : 0;

            // Update win rate
            updatedStats.WinRate = updatedStats.TotalGames > 0 
                ? (double)updatedStats.Wins / updatedStats.TotalGames 
                : 0;

            // Save to storage
            await _storageService.SavePlayerStatsAsync(player.Name, updatedStats);
        }
    }
    */
}