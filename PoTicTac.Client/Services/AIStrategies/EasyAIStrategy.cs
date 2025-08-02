using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class EasyAIStrategy : IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameState gameState)
    {
        var random = new Random();
        var moves = GetAvailableMoves(gameState.Board);
        return Task.FromResult(moves[random.Next(moves.Count)]);
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
