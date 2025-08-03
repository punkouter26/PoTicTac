using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class MediumAIStrategy : IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameBoardState gameState)
    {
        // First try to win
        var winningMove = FindWinningMove(gameState.Board, gameState.CurrentPlayer);
        if (winningMove != null)
            return Task.FromResult(winningMove);

        // Then block opponent's winning move
        var opponent = gameState.CurrentPlayer == PlayerType.X ? PlayerType.O : PlayerType.X;
        var blockingMove = FindWinningMove(gameState.Board, opponent);
        if (blockingMove != null)
            return Task.FromResult(blockingMove);

        // Otherwise make a random move
        var random = new Random();
        var moves = GetAvailableMoves(gameState.Board);
        return Task.FromResult(moves[random.Next(moves.Count)]);
    }

    private int[]? FindWinningMove(PlayerType[,] board, PlayerType player)
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == PlayerType.None)
                return new[] { i, 2 };
            if (board[i, 0] == player && board[i, 2] == player && board[i, 1] == PlayerType.None)
                return new[] { i, 1 };
            if (board[i, 1] == player && board[i, 2] == player && board[i, 0] == PlayerType.None)
                return new[] { i, 0 };
        }

        // Check columns
        for (int j = 0; j < 3; j++)
        {
            if (board[0, j] == player && board[1, j] == player && board[2, j] == PlayerType.None)
                return new[] { 2, j };
            if (board[0, j] == player && board[2, j] == player && board[1, j] == PlayerType.None)
                return new[] { 1, j };
            if (board[1, j] == player && board[2, j] == player && board[0, j] == PlayerType.None)
                return new[] { 0, j };
        }

        // Check diagonals
        if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == PlayerType.None)
            return new[] { 2, 2 };
        if (board[0, 0] == player && board[2, 2] == player && board[1, 1] == PlayerType.None)
            return new[] { 1, 1 };
        if (board[1, 1] == player && board[2, 2] == player && board[0, 0] == PlayerType.None)
            return new[] { 0, 0 };

        if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == PlayerType.None)
            return new[] { 2, 0 };
        if (board[0, 2] == player && board[2, 0] == player && board[1, 1] == PlayerType.None)
            return new[] { 1, 1 };
        if (board[1, 1] == player && board[2, 0] == player && board[0, 2] == PlayerType.None)
            return new[] { 0, 2 };

        return null;
    }

    private List<int[]> GetAvailableMoves(PlayerType[,] board)
    {
        var moves = new List<int[]>();
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == PlayerType.None)
                {
                    moves.Add(new[] { i, j });
                }
            }
        }
        return moves;
    }
}
