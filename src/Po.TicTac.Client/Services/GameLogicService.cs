using PoTicTac.Client.Models;
using PoTicTac.Client.Services.WinStrategies;

namespace PoTicTac.Client.Services;

public class GameLogicService
{
    private readonly WinChecker _winChecker;

    public GameLogicService()
    {
        _winChecker = new WinChecker();
    }
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
        return _winChecker.CheckWinner(board);
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
