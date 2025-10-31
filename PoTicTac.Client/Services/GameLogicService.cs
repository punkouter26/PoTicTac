using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services;

public class GameLogicService
{
    public GameBoardState CreateInitialState(PlayerType startingPlayer, Player[] players)
    {
        var initialState = new GameBoardState
        {
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

        // The constructor already initializes the board to all None values
        return initialState;
    }

    public PlayerType? CheckWinner(GameBoardState board)
    {
        const int winLength = 4; // Need 4 in a row to win
        int boardSize = GameBoardState.BoardSize;

        // Check rows for 4 in a row
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                PlayerType first = board.Board[row, col];
                if (first != PlayerType.None)
                {
                    bool win = true;
                    for (int i = 1; i < winLength; i++)
                    {
                        if (board.Board[row, col + i] != first)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        return first;
                    }
                }
            }
        }

        // Check columns for 4 in a row
        for (int col = 0; col < boardSize; col++)
        {
            for (int row = 0; row <= boardSize - winLength; row++)
            {
                PlayerType first = board.Board[row, col];
                if (first != PlayerType.None)
                {
                    bool win = true;
                    for (int i = 1; i < winLength; i++)
                    {
                        if (board.Board[row + i, col] != first)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        return first;
                    }
                }
            }
        }

        // Check diagonals (top-left to bottom-right)
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                PlayerType first = board.Board[row, col];
                if (first != PlayerType.None)
                {
                    bool win = true;
                    for (int i = 1; i < winLength; i++)
                    {
                        if (board.Board[row + i, col + i] != first)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        return first;
                    }
                }
            }
        }

        // Check diagonals (top-right to bottom-left)
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = winLength - 1; col < boardSize; col++)
            {
                PlayerType first = board.Board[row, col];
                if (first != PlayerType.None)
                {
                    bool win = true;
                    for (int i = 1; i < winLength; i++)
                    {
                        if (board.Board[row + i, col - i] != first)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win)
                    {
                        return first;
                    }
                }
            }
        }

        // Check for draw (no empty cells)
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                if (board.Board[row, col] == PlayerType.None)
                {
                    return null; // Game still in progress
                }
            }
        }

        return PlayerType.Draw;
    }

    public GameBoardState MakeMove(GameBoardState state, int row, int col)
    {
        // If game is not in playing state or cell is already occupied
        if (state.GameStatus != GameStatus.Playing || state.Board[row, col] != PlayerType.None)
        {
            return state;
        }

        // Create new state with the move
        var newState = new GameBoardState
        {
            Board = (PlayerType[,])state.Board.Clone(),
            CurrentPlayer = state.CurrentPlayer,
            GameStatus = state.GameStatus,
            Winner = state.Winner,
            Players = state.Players,
            MoveHistory = new List<Move>(state.MoveHistory),
            UndoStack = state.UndoStack,
            RedoStack = state.RedoStack,
            SessionStats = state.SessionStats
        };

        // Apply the move
        newState.Board[row, col] = state.CurrentPlayer;

        // Add move to history
        newState.MoveHistory.Add(new Move
        {
            Player = state.CurrentPlayer,
            Position = new[] { row, col },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        // Check for winner or draw
        var winner = CheckWinner(newState);
        if (winner != null)
        {
            newState.GameStatus = winner == PlayerType.Draw ? GameStatus.Draw : GameStatus.Won;
            newState.Winner = winner == PlayerType.Draw ? null : winner;
        }

        // Switch player if game is still in progress
        if (newState.GameStatus == GameStatus.Playing)
        {
            newState.CurrentPlayer = state.CurrentPlayer == PlayerType.X ? PlayerType.O : PlayerType.X;
        }

        return newState;
    }

    public GameBoardState UndoMove(GameBoardState state)
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

        // Create new state with the move undone
        var newState = new GameBoardState
        {
            Board = (PlayerType[,])state.Board.Clone(),
            CurrentPlayer = state.CurrentPlayer,
            GameStatus = state.GameStatus,
            Winner = state.Winner,
            Players = state.Players,
            MoveHistory = new List<Move>(state.MoveHistory),
            UndoStack = new List<Move>(state.UndoStack),
            RedoStack = new List<Move>(state.RedoStack),
            SessionStats = state.SessionStats
        };

        // Undo the move
        newState.Board[row, col] = PlayerType.None;
        newState.MoveHistory.RemoveAt(newState.MoveHistory.Count - 1);
        newState.UndoStack.Add(lastMove);

        // Switch back to the player who made the move
        newState.CurrentPlayer = lastMove.Player;

        return newState;
    }

    public GameBoardState RedoMove(GameBoardState state)
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

        // Create new state with the move redone
        var newState = new GameBoardState
        {
            Board = (PlayerType[,])state.Board.Clone(),
            CurrentPlayer = state.CurrentPlayer,
            GameStatus = state.GameStatus,
            Winner = state.Winner,
            Players = state.Players,
            MoveHistory = new List<Move>(state.MoveHistory),
            UndoStack = new List<Move>(state.UndoStack),
            RedoStack = new List<Move>(state.RedoStack),
            SessionStats = state.SessionStats
        };

        // Redo the move
        newState.Board[row, col] = moveToRedo.Player;
        newState.MoveHistory.Add(moveToRedo);
        newState.UndoStack.RemoveAt(newState.UndoStack.Count - 1);

        // Check for winner or draw
        var winner = CheckWinner(newState);
        if (winner != null)
        {
            newState.GameStatus = winner == PlayerType.Draw ? GameStatus.Draw : GameStatus.Won;
            newState.Winner = winner == PlayerType.Draw ? null : winner;
        }

        // Switch player if game is still in progress
        if (newState.GameStatus == GameStatus.Playing)
        {
            newState.CurrentPlayer = moveToRedo.Player == PlayerType.X ? PlayerType.O : PlayerType.X;
        }

        return newState;
    }
}
