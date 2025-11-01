using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.WinStrategies;

/// <summary>
/// Orchestrates win checking using strategy pattern.
/// Reduces cyclomatic complexity from ~18 to ~4 per strategy.
/// Follows Single Responsibility and Open/Closed principles.
/// </summary>
public class WinChecker
{
    private readonly List<IWinDirectionStrategy> _strategies;
    private const int WinLength = 4; // Need 4 in a row to win

    public WinChecker()
    {
        _strategies = new List<IWinDirectionStrategy>
        {
            new RowWinStrategy(),
            new ColumnWinStrategy(),
            new DiagonalDownWinStrategy(),
            new DiagonalUpWinStrategy()
        };
    }

    /// <summary>
    /// Checks all win directions and returns the winner, or null if no winner yet.
    /// Returns PlayerType.Draw if the board is full with no winner.
    /// </summary>
    public PlayerType? CheckWinner(GameBoardState board)
    {
        // Check each direction strategy
        foreach (var strategy in _strategies)
        {
            var winner = strategy.CheckDirection(board, WinLength);
            if (winner != null)
            {
                return winner;
            }
        }

        // Check for draw (no empty cells)
        return CheckDraw(board);
    }

    /// <summary>
    /// Checks if the board is full (draw condition).
    /// </summary>
    private static PlayerType? CheckDraw(GameBoardState board)
    {
        int boardSize = GameBoardState.BoardSize;

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
}
