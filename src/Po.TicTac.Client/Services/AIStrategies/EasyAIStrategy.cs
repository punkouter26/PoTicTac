using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class EasyAIStrategy : IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameBoardState gameState)
    {
        var random = new Random();
        var moves = GetAvailableMoves(gameState.Board);
        return Task.FromResult(moves[random.Next(moves.Count)]);
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
