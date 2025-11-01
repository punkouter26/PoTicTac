using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.WinStrategies;

/// <summary>
/// Strategy interface for checking win conditions in a specific direction.
/// </summary>
public interface IWinDirectionStrategy
{
    /// <summary>
    /// Checks if there is a winner in the specific direction handled by this strategy.
    /// </summary>
    /// <param name="board">The game board state to check.</param>
    /// <param name="winLength">The number of consecutive pieces required to win.</param>
    /// <returns>The winning player type, or null if no winner in this direction.</returns>
    PlayerType? CheckDirection(GameBoardState board, int winLength);
}
