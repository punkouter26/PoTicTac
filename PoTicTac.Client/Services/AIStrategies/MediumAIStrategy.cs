using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class MediumAIStrategy : IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameState gameState)
    {
        // First check for immediate win
        var winningMove = FindWinningMove(gameState.Board, gameState.CurrentPlayer);
        if (winningMove != null) return Task.FromResult(winningMove);

        // Then block opponent's winning move
        var blockingMove = FindWinningMove(gameState.Board, gameState.CurrentPlayer == 1 ? 2 : 1);
        if (blockingMove != null) return Task.FromResult(blockingMove);

        // Otherwise make a random move
        var random = new Random();
        var moves = GetAvailableMoves(gameState.Board);
        return Task.FromResult(moves[random.Next(moves.Count)]);
    }

    private int[]? FindWinningMove(int[][] board, int player)
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (board[i][0] == player && board[i][1] == player && board[i][2] == 0)
                return new[] { i, 2 };
            if (board[i][0] == player && board[i][2] == player && board[i][1] == 0)
                return new[] { i, 1 };
            if (board[i][1] == player && board[i][2] == player && board[i][0] == 0)
                return new[] { i, 0 };
        }

        // Check columns
        for (int j = 0; j < 3; j++)
        {
            if (board[0][j] == player && board[1][j] == player && board[2][j] == 0)
                return new[] { 2, j };
            if (board[0][j] == player && board[2][j] == player && board[1][j] == 0)
                return new[] { 1, j };
            if (board[1][j] == player && board[2][j] == player && board[0][j] == 0)
                return new[] { 0, j };
        }

        // Check diagonals
        if (board[0][0] == player && board[1][1] == player && board[2][2] == 0)
            return new[] { 2, 2 };
        if (board[0][0] == player && board[2][2] == player && board[1][1] == 0)
            return new[] { 1, 1 };
        if (board[1][1] == player && board[2][2] == player && board[0][0] == 0)
            return new[] { 0, 0 };
        if (board[0][2] == player && board[1][1] == player && board[2][0] == 0)
            return new[] { 2, 0 };
        if (board[0][2] == player && board[2][0] == player && board[1][1] == 0)
            return new[] { 1, 1 };
        if (board[1][1] == player && board[2][0] == player && board[0][2] == 0)
            return new[] { 0, 2 };

        return null;
    }

    private List<int[]> GetAvailableMoves(int[][] board)
    {
        var moves = new List<int[]>();
        for (int i = 0; i < board.Length; i++)
        {
            for (int j = 0; j < board[i].Length; j++)
            {
                if (board[i][j] == 0)
                {
                    moves.Add(new[] { i, j });
                }
            }
        }
        return moves;
    }
}
