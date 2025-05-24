using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services
{
    public class GameLogicService
    {
        public const int BOARD_SIZE = 6;
        public const int WIN_LENGTH = 4;
        public const int TURN_TIME_LIMIT = 5000; // 5 seconds in milliseconds

        public GameState CreateInitialState(int startingPlayer, Player[] players)
        {
            var board = new int[BOARD_SIZE][];
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                board[i] = new int[BOARD_SIZE];
            }

            return new GameState
            {
                Board = board,
                CurrentPlayer = startingPlayer,
                GameStatus = GameStatus.Playing,
                Winner = null,
                Players = players,
                MoveHistory = new List<Move>(),
                UndoStack = new List<Move>(),
                RedoStack = new List<Move>(),
                SessionStats = new SessionStats
                {
                    TotalGames = 0,
                    StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    LastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };
        }

        public bool IsValidPosition(int row, int col)
        {
            return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
        }

        public List<int[]>? CheckWinner(int[][] board, int[] lastMove)
        {
            var row = lastMove[0];
            var col = lastMove[1];
            var player = board[row][col];
            var directions = new int[][]
            {
                new[] { 0, 1 },  // horizontal
                new[] { 1, 0 },  // vertical
                new[] { 1, 1 },  // diagonal \
                new[] { 1, -1 }  // diagonal /
            };

            foreach (var direction in directions)
            {
                var dx = direction[0];
                var dy = direction[1];
                var line = new List<int[]> { new[] { row, col } };

                // Check in positive direction
                for (int i = 1; i < WIN_LENGTH; i++)
                {
                    var newRow = row + dx * i;
                    var newCol = col + dy * i;
                    if (!IsValidPosition(newRow, newCol) || board[newRow][newCol] != player)
                        break;
                    line.Add(new[] { newRow, newCol });
                }

                // Check in negative direction
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

        public GameState MakeMove(GameState state, int row, int col)
        {
            // If game is not in playing state or cell is already occupied
            if (state.GameStatus != GameStatus.Playing || state.Board[row][col] != 0)
            {
                return state;
            }

            // Create new board with the move
            var newBoard = new int[BOARD_SIZE][];
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                newBoard[i] = new int[BOARD_SIZE];
                Array.Copy(state.Board[i], newBoard[i], BOARD_SIZE);
            }
            newBoard[row][col] = state.CurrentPlayer;

            // Add move to history
            var newMoveHistory = new List<Move>(state.MoveHistory)
            {
                new Move
                {
                    Player = state.CurrentPlayer,
                    Position = new[] { row, col },
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };

            // Check for win
            var winningCells = CheckWinner(newBoard, new[] { row, col });
            if (winningCells != null)
            {
                return new GameState
                {
                    Board = newBoard,
                    GameStatus = GameStatus.Won,
                    Winner = state.CurrentPlayer,
                    WinningCells = winningCells,
                    CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
                    Players = state.Players,
                    MoveHistory = newMoveHistory,
                    UndoStack = state.UndoStack,
                    RedoStack = state.RedoStack,
                    SessionStats = state.SessionStats
                };
            }

            // Check for draw
            if (IsDraw(newBoard))
            {
                return new GameState
                {
                    Board = newBoard,
                    GameStatus = GameStatus.Draw,
                    Winner = null,
                    CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
                    Players = state.Players,
                    MoveHistory = newMoveHistory,
                    UndoStack = state.UndoStack,
                    RedoStack = state.RedoStack,
                    SessionStats = state.SessionStats
                };
            }

            // Continue game
            return new GameState
            {
                Board = newBoard,
                CurrentPlayer = state.CurrentPlayer == 1 ? 2 : 1,
                GameStatus = GameStatus.Playing,
                Winner = null,
                Players = state.Players,
                MoveHistory = newMoveHistory,
                UndoStack = state.UndoStack,
                RedoStack = state.RedoStack,
                SessionStats = state.SessionStats
            };
        }

        private bool IsDraw(int[][] board)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (board[i][j] == 0)
                        return false;
                }
            }
            return true;
        }

        public GameState UndoMove(GameState state)
        {
            // If no moves to undo or game is won/drawn, return current state
            if (state.MoveHistory.Count == 0 || state.GameStatus != GameStatus.Playing)
            {
                return state;
            }

            // Get the last move
            var lastMove = state.MoveHistory[^1];
            var row = lastMove.Position[0];
            var col = lastMove.Position[1];

            // Create new board with the move undone
            var newBoard = new int[BOARD_SIZE][];
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                newBoard[i] = new int[BOARD_SIZE];
                Array.Copy(state.Board[i], newBoard[i], BOARD_SIZE);
            }
            newBoard[row][col] = 0;

            // Update move history and undo/redo stacks
            var newMoveHistory = state.MoveHistory.Take(state.MoveHistory.Count - 1).ToList();
            var newUndoStack = new List<Move>(state.UndoStack) { lastMove };

            return new GameState
            {
                Board = newBoard,
                CurrentPlayer = lastMove.Player, // Switch back to the player who made the move
                GameStatus = GameStatus.Playing,
                Winner = null,
                Players = state.Players,
                MoveHistory = newMoveHistory,
                UndoStack = newUndoStack,
                RedoStack = new List<Move>(),
                SessionStats = state.SessionStats
            };
        }

        public GameState RedoMove(GameState state)
        {
            // If no moves to redo or game is won/drawn, return current state
            if (state.UndoStack.Count == 0 || state.GameStatus != GameStatus.Playing)
            {
                return state;
            }

            // Get the last undone move
            var moveToRedo = state.UndoStack[^1];
            var row = moveToRedo.Position[0];
            var col = moveToRedo.Position[1];

            // Create new board with the move redone
            var newBoard = new int[BOARD_SIZE][];
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                newBoard[i] = new int[BOARD_SIZE];
                Array.Copy(state.Board[i], newBoard[i], BOARD_SIZE);
            }
            newBoard[row][col] = moveToRedo.Player;

            // Update move history and undo/redo stacks
            var newMoveHistory = new List<Move>(state.MoveHistory) { moveToRedo };
            var newUndoStack = state.UndoStack.Take(state.UndoStack.Count - 1).ToList();

            // Check for win after redoing the move
            var winningCells = CheckWinner(newBoard, new[] { row, col });
            if (winningCells != null)
            {
                return new GameState
                {
                    Board = newBoard,
                    GameStatus = GameStatus.Won,
                    Winner = moveToRedo.Player,
                    WinningCells = winningCells,
                    CurrentPlayer = moveToRedo.Player == 1 ? 2 : 1,
                    Players = state.Players,
                    MoveHistory = newMoveHistory,
                    UndoStack = newUndoStack,
                    RedoStack = new List<Move>(),
                    SessionStats = state.SessionStats
                };
            }

            // Check for draw
            if (IsDraw(newBoard))
            {
                return new GameState
                {
                    Board = newBoard,
                    GameStatus = GameStatus.Draw,
                    Winner = null,
                    CurrentPlayer = moveToRedo.Player == 1 ? 2 : 1,
                    Players = state.Players,
                    MoveHistory = newMoveHistory,
                    UndoStack = newUndoStack,
                    RedoStack = new List<Move>(),
                    SessionStats = state.SessionStats
                };
            }

            return new GameState
            {
                Board = newBoard,
                CurrentPlayer = moveToRedo.Player == 1 ? 2 : 1,
                GameStatus = GameStatus.Playing,
                Winner = null,
                Players = state.Players,
                MoveHistory = newMoveHistory,
                UndoStack = newUndoStack,
                RedoStack = new List<Move>(),
                SessionStats = state.SessionStats
            };
        }
    }
}
