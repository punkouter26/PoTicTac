using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.WinStrategies;

/// <summary>
/// Strategy for checking diagonal wins (top-right to bottom-left direction).
/// Complexity: ~4 (down from 18 in monolithic approach)
/// </summary>
public class DiagonalUpWinStrategy : IWinDirectionStrategy
{
    public PlayerType? CheckDirection(GameBoardState board, int winLength)
    {
        int boardSize = GameBoardState.BoardSize;

        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = winLength - 1; col < boardSize; col++)
            {
                PlayerType first = board.Board[row, col];
                if (first == PlayerType.None)
                {
                    continue;
                }

                if (CheckConsecutive(board, row, col, 1, -1, winLength, first))
                {
                    return first;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Helper to check consecutive cells in a specific direction.
    /// </summary>
    private static bool CheckConsecutive(GameBoardState board, int startRow, int startCol,
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
}
