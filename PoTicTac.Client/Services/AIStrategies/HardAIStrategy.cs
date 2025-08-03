using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class HardAIStrategy : IAIStrategy
{
    public Task<int[]> GetMoveAsync(GameBoardState gameState)
    {
        var bestMove = FindBestMove(gameState.Board, gameState.CurrentPlayer);
        return Task.FromResult(bestMove);
    }

    private int[] FindBestMove(PlayerType[,] board, PlayerType player)
    {
        int bestScore = int.MinValue;
        int[] bestMove = new int[2];

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == PlayerType.None)
                {
                    board[i, j] = player;
                    int score = Minimax(board, 0, false, player);
                    board[i, j] = PlayerType.None;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new[] { i, j };
                    }
                }
            }
        }

        return bestMove;
    }

    private int Minimax(PlayerType[,] board, int depth, bool isMaximizing, PlayerType player)
    {
        PlayerType opponent = player == PlayerType.X ? PlayerType.O : PlayerType.X;
        var result = CheckWinner(board);

        if (result != PlayerType.None)
        {
            return result == player ? 10 - depth : depth - 10;
        }

        if (IsBoardFull(board))
        {
            return 0;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == PlayerType.None)
                    {
                        board[i, j] = player;
                        int score = Minimax(board, depth + 1, false, player);
                        board[i, j] = PlayerType.None;
                        bestScore = Math.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == PlayerType.None)
                    {
                        board[i, j] = opponent;
                        int score = Minimax(board, depth + 1, true, player);
                        board[i, j] = PlayerType.None;
                        bestScore = Math.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    private PlayerType CheckWinner(PlayerType[,] board)
    {
        // Check rows
        for (int i = 0; i < 3; i++)
        {
            if (board[i, 0] != PlayerType.None && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                return board[i, 0];
        }

        // Check columns
        for (int j = 0; j < 3; j++)
        {
            if (board[0, j] != PlayerType.None && board[0, j] == board[1, j] && board[1, j] == board[2, j])
                return board[0, j];
        }

        // Check diagonals
        if (board[0, 0] != PlayerType.None && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            return board[0, 0];

        if (board[0, 2] != PlayerType.None && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            return board[0, 2];

        return PlayerType.None;
    }

    private bool IsBoardFull(PlayerType[,] board)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == PlayerType.None)
                    return false;
            }
        }
        return true;
    }
}
