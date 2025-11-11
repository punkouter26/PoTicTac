using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.WinStrategies;

/// <summary>
/// Checks for winning conditions on the game board.
/// Checks all four directions (row, column, diagonal-down, diagonal-up) from each position.
/// </summary>
public class WinChecker
{
    private const int WinLength = 4; // Need 4 in a row to win

    /// <summary>
    /// Checks all win directions and returns the winner, or null if no winner yet.
    /// Returns PlayerType.Draw if the board is full with no winner.
    /// </summary>
    public PlayerType? CheckWinner(GameBoardState board)
    {
        int boardSize = GameBoardState.BoardSize;

        // Check all positions for potential winning sequences
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                PlayerType cell = board.Board[row, col];
                if (cell == PlayerType.None)
                {
                    continue;
                }

                // Check horizontal (row)
                if (col <= boardSize - WinLength && CheckLine(board, row, col, 0, 1, WinLength, cell))
                {
                    return cell;
                }

                // Check vertical (column)
                if (row <= boardSize - WinLength && CheckLine(board, row, col, 1, 0, WinLength, cell))
                {
                    return cell;
                }

                // Check diagonal down-right (\)
                if (row <= boardSize - WinLength && col <= boardSize - WinLength &&
                    CheckLine(board, row, col, 1, 1, WinLength, cell))
                {
                    return cell;
                }

                // Check diagonal up-right (/)
                if (row >= WinLength - 1 && col <= boardSize - WinLength &&
                    CheckLine(board, row, col, -1, 1, WinLength, cell))
                {
                    return cell;
                }
            }
        }

        // Check for draw (no empty cells)
        return CheckDraw(board);
    }

    /// <summary>
    /// Checks if there are consecutive cells of the same player in a specific direction.
    /// </summary>
    private static bool CheckLine(GameBoardState board, int startRow, int startCol,
        int rowDelta, int colDelta, int winLength, PlayerType player)
    {
        for (int i = 1; i < winLength; i++)
        {
            int newRow = startRow + (i * rowDelta);
            int newCol = startCol + (i * colDelta);

            if (board.Board[newRow, newCol] != player)
            {
                return false;
            }
        }

        return true;
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
