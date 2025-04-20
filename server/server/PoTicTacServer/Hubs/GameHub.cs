using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoTicTacServer.Hubs
{
    // Game state representation
    public class GameState
    {
        public string GameId { get; set; }
        public List<List<int>> Board { get; set; }
        public int CurrentPlayer { get; set; }
        public string GameStatus { get; set; }
        public int? Winner { get; set; }
        public List<Player> Players { get; set; }
        public List<Move> MoveHistory { get; set; }
        public List<int[]> WinningCells { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Symbol { get; set; }
        public PlayerStats Stats { get; set; }
    }

    public class PlayerStats
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalGames { get; set; }
        public int WinStreak { get; set; }
        public int CurrentStreak { get; set; }
        public double AverageMovesPerGame { get; set; }
        public int TotalMoves { get; set; }
        public double WinRate { get; set; }
    }

    public class Move
    {
        public int Player { get; set; }
        public int[] Position { get; set; }
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
                WinningCells = null
            };
        }

        public static bool IsValidMove(GameState state, int row, int col)
        {
            if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE)
                return false;

            if (state.Board[row][col] != 0)
                return false;

            if (state.GameStatus != "playing")
                return false;

            return true;
        }

        public static GameState MakeMove(GameState state, int row, int col)
        {
            if (!IsValidMove(state, row, col))
                return state;

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
                    WinningCells = null
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
                WinningCells = null
            };
        }

        private static List<int[]> CheckWinner(List<List<int>> board, int row, int col)
        {
            var player = board[row][col];
            var directions = new[] { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };

            foreach (var dir in directions)
            {
                var dx = dir[0];
                var dy = dir[1];
                var line = new List<int[]> { new[] { row, col } };

                // Check positive direction
                for (int i = 1; i < WIN_LENGTH; i++)
                {
                    var newRow = row + dx * i;
                    var newCol = col + dy * i;
                    if (!IsValidPosition(newRow, newCol) || board[newRow][newCol] != player)
                        break;
                    line.Add(new[] { newRow, newCol });
                }

                // Check negative direction
                for (int i = 1; i < WIN_LENGTH; i++)
                {
                    var newRow = row - dx * i;
                    var newCol = col - dy * i;
                    if (!IsValidPosition(newRow, newCol) || board[newRow][newCol] != player)
                        break;
                    line.Add(new[] { newRow, newCol });
                }

                if (line.Count >= WIN_LENGTH)
                    return line;
            }

            return null;
        }

        private static bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
        }

        private static bool IsDraw(List<List<int>> board)
        {
            return board.All(row => row.All(cell => cell != 0));
        }
    }

    public class GameHub : Hub
    {
        private static readonly ConcurrentDictionary<string, GameState> Games = new ConcurrentDictionary<string, GameState>();
        private static readonly ConcurrentDictionary<string, string> UserGameMap = new ConcurrentDictionary<string, string>();

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
                Stats = new PlayerStats
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
                }
            };

            var gameState = GameLogic.CreateInitialState(gameId, player);
            Games[gameId] = gameState;
            UserGameMap[playerId] = gameId;

            await Groups.AddToGroupAsync(playerId, gameId);
            await Clients.Caller.SendAsync("GameCreated", gameId);

            return gameId;
        }

        public async Task JoinGame(string gameId, string playerName)
        {
            if (!Games.TryGetValue(gameId, out var gameState))
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
                Stats = new PlayerStats
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
                }
            };

            gameState.Players.Add(player);
            gameState.GameStatus = "playing";
            UserGameMap[playerId] = gameId;

            await Groups.AddToGroupAsync(playerId, gameId);
            
            // Notify existing player about the new player
            await Clients.Group(gameId).SendAsync("PlayerJoined", player);
            
            // Send the complete game state to both players
            await Clients.Group(gameId).SendAsync("GameJoined", gameState, gameState.Players);
        }

        public async Task MakeMove(string gameId, int row, int col)
        {
            if (!Games.TryGetValue(gameId, out var gameState))
            {
                throw new HubException("Game not found");
            }

            var playerId = Context.ConnectionId;
            var player = gameState.Players.FirstOrDefault(p => p.Id == playerId);
            
            if (player == null)
            {
                throw new HubException("Player not found in this game");
            }

            if (player.Symbol != gameState.CurrentPlayer)
            {
                throw new HubException("It's not your turn");
            }

            if (!GameLogic.IsValidMove(gameState, row, col))
            {
                throw new HubException("Invalid move");
            }

            var newGameState = GameLogic.MakeMove(gameState, row, col);
            Games[gameId] = newGameState;

            var lastMove = newGameState.MoveHistory.LastOrDefault();
            await Clients.Group(gameId).SendAsync("MoveReceived", newGameState, lastMove);

            if (newGameState.GameStatus != "playing")
            {
                await Clients.Group(gameId).SendAsync("GameOver", newGameState);
            }
        }

        public async Task RequestRematch(string gameId)
        {
            if (!Games.TryGetValue(gameId, out var gameState))
            {
                throw new HubException("Game not found");
            }

            var playerId = Context.ConnectionId;
            await Clients.OthersInGroup(gameId).SendAsync("RematchRequested", playerId);
        }

        public async Task AcceptRematch(string gameId)
        {
            if (!Games.TryGetValue(gameId, out var gameState))
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
                WinningCells = null
            };

            Games[gameId] = newGameState;
            await Clients.Group(gameId).SendAsync("RematchAccepted", newGameState);
        }

        public async Task LeaveGame(string gameId)
        {
            var playerId = Context.ConnectionId;
            
            if (UserGameMap.TryRemove(playerId, out _))
            {
                await Groups.RemoveFromGroupAsync(playerId, gameId);
                await Clients.OthersInGroup(gameId).SendAsync("PlayerLeft", playerId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var playerId = Context.ConnectionId;
            
            if (UserGameMap.TryRemove(playerId, out var gameId))
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
    }
}