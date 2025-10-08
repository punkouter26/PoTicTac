using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services.AIStrategies;

public class HardAIStrategy : IAIStrategy
{
    // For 6x6 board, we MUST limit depth severely or use heuristic evaluation
    private const int MaxDepth = 4; // Reduced from 9 - critically important for 6x6 board
    private readonly Dictionary<string, int> _transpositionTable = new();

    public Task<int[]> GetMoveAsync(GameBoardState gameState)
    {
        _transpositionTable.Clear(); // Clear cache for new game state
        var bestMove = FindBestMove(gameState.Board, gameState.CurrentPlayer);
        return Task.FromResult(bestMove);
    }

    private int[] FindBestMove(PlayerType[,] board, PlayerType player)
    {
        int boardSize = board.GetLength(0);
        
        // Optimization: If board is empty, take center
        if (IsEmptyBoard(board))
        {
            int center = boardSize / 2;
            return new[] { center, center };
        }

        // Check for immediate winning move or blocking move first
        var winningMove = FindImmediateWinOrBlock(board, player);
        if (winningMove != null)
        {
            return winningMove;
        }

        int bestScore = int.MinValue;
        int[]? bestMove = null;

        // Evaluate moves with priority ordering (center and adjacent cells first)
        var moves = GetOrderedMoves(board, player);
        
        foreach (var move in moves.Take(Math.Min(10, moves.Count))) // Limit to top 10 moves for performance
        {
            int i = move[0];
            int j = move[1];
            
            board[i, j] = player;
            int score = Minimax(board, 0, false, player, int.MinValue, int.MaxValue);
            board[i, j] = PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = new[] { i, j };
            }
        }

        return bestMove ?? new[] { boardSize / 2, boardSize / 2 };
    }

    private int[]? FindImmediateWinOrBlock(PlayerType[,] board, PlayerType player)
    {
        int boardSize = board.GetLength(0);
        PlayerType opponent = player == PlayerType.X ? PlayerType.O : PlayerType.X;

        // Check for winning move first
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i, j] == PlayerType.None)
                {
                    board[i, j] = player;
                    if (CheckWinner(board) == player)
                    {
                        board[i, j] = PlayerType.None;
                        return new[] { i, j };
                    }
                    board[i, j] = PlayerType.None;
                }
            }
        }

        // Check for blocking opponent's winning move
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i, j] == PlayerType.None)
                {
                    board[i, j] = opponent;
                    if (CheckWinner(board) == opponent)
                    {
                        board[i, j] = PlayerType.None;
                        return new[] { i, j };
                    }
                    board[i, j] = PlayerType.None;
                }
            }
        }

        return null;
    }

    private List<int[]> GetOrderedMoves(PlayerType[,] board, PlayerType player)
    {
        int boardSize = board.GetLength(0);
        var moves = new List<(int[] move, int priority)>();
        int center = boardSize / 2;

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i, j] == PlayerType.None)
                {
                    // Priority based on distance from center and adjacency to existing pieces
                    int distanceFromCenter = Math.Abs(i - center) + Math.Abs(j - center);
                    int adjacentPieces = CountAdjacentPieces(board, i, j, player);
                    int priority = (100 - distanceFromCenter) + (adjacentPieces * 50);
                    
                    moves.Add((new[] { i, j }, priority));
                }
            }
        }

        return moves.OrderByDescending(m => m.priority).Select(m => m.move).ToList();
    }

    private int CountAdjacentPieces(PlayerType[,] board, int row, int col, PlayerType player)
    {
        int count = 0;
        int boardSize = board.GetLength(0);
        int[][] directions = new int[][] {
            new[] {-1, 0}, new[] {1, 0}, new[] {0, -1}, new[] {0, 1},
            new[] {-1, -1}, new[] {-1, 1}, new[] {1, -1}, new[] {1, 1}
        };

        foreach (var dir in directions)
        {
            int newRow = row + dir[0];
            int newCol = col + dir[1];
            if (newRow >= 0 && newRow < boardSize && newCol >= 0 && newCol < boardSize &&
                board[newRow, newCol] == player)
            {
                count++;
            }
        }

        return count;
    }

    private string GetBoardHash(PlayerType[,] board)
    {
        int boardSize = board.GetLength(0);
        var chars = new char[boardSize * boardSize];
        int index = 0;
        
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                chars[index++] = board[i, j] switch
                {
                    PlayerType.X => 'X',
                    PlayerType.O => 'O',
                    _ => '-'
                };
            }
        }
        
        return new string(chars);
    }

    private int Minimax(PlayerType[,] board, int depth, bool isMaximizing, PlayerType player, int alpha, int beta)
    {
        // Check transposition table
        string boardHash = GetBoardHash(board);
        if (_transpositionTable.TryGetValue(boardHash + depth, out int cachedScore))
        {
            return cachedScore;
        }

        // Depth limit check - use heuristic evaluation
        if (depth >= MaxDepth)
        {
            int heuristicScore = EvaluatePosition(board, player);
            _transpositionTable[boardHash + depth] = heuristicScore;
            return heuristicScore;
        }

        PlayerType opponent = player == PlayerType.X ? PlayerType.O : PlayerType.X;
        var result = CheckWinner(board);

        if (result != PlayerType.None)
        {
            int score = result == player ? 1000 - depth : depth - 1000;
            _transpositionTable[boardHash + depth] = score;
            return score;
        }

        if (IsBoardFull(board))
        {
            _transpositionTable[boardHash + depth] = 0;
            return 0;
        }

        int boardSize = board.GetLength(0);

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == PlayerType.None)
                    {
                        board[i, j] = player;
                        int score = Minimax(board, depth + 1, false, player, alpha, beta);
                        board[i, j] = PlayerType.None;
                        bestScore = Math.Max(score, bestScore);
                        alpha = Math.Max(alpha, bestScore);
                        
                        // Alpha-beta pruning
                        if (beta <= alpha)
                        {
                            _transpositionTable[boardHash + depth] = bestScore;
                            return bestScore;
                        }
                    }
                }
            }
            _transpositionTable[boardHash + depth] = bestScore;
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == PlayerType.None)
                    {
                        board[i, j] = opponent;
                        int score = Minimax(board, depth + 1, true, player, alpha, beta);
                        board[i, j] = PlayerType.None;
                        bestScore = Math.Min(score, bestScore);
                        beta = Math.Min(beta, bestScore);
                        
                        // Alpha-beta pruning
                        if (beta <= alpha)
                        {
                            _transpositionTable[boardHash + depth] = bestScore;
                            return bestScore;
                        }
                    }
                }
            }
            _transpositionTable[boardHash + depth] = bestScore;
            return bestScore;
        }
    }

    private int EvaluatePosition(PlayerType[,] board, PlayerType player)
    {
        // Heuristic evaluation for positions that reach depth limit
        int score = 0;
        int boardSize = board.GetLength(0);
        PlayerType opponent = player == PlayerType.X ? PlayerType.O : PlayerType.X;

        // Evaluate all possible lines
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                if (board[i, j] == player)
                {
                    // Check potential lines from this position
                    score += EvaluateLinesFromPosition(board, i, j, player);
                }
                else if (board[i, j] == opponent)
                {
                    score -= EvaluateLinesFromPosition(board, i, j, opponent);
                }
            }
        }

        return score;
    }

    private int EvaluateLinesFromPosition(PlayerType[,] board, int row, int col, PlayerType player)
    {
        int score = 0;
        int boardSize = board.GetLength(0);
        
        // Check horizontal, vertical, and diagonal lines
        int[][] directions = new int[][] {
            new[] {0, 1},   // Horizontal
            new[] {1, 0},   // Vertical
            new[] {1, 1},   // Diagonal \
            new[] {1, -1}   // Diagonal /
        };

        foreach (var dir in directions)
        {
            int count = 1; // Current piece
            int empty = 0;
            
            // Check in positive direction
            for (int k = 1; k < 3; k++)
            {
                int newRow = row + dir[0] * k;
                int newCol = col + dir[1] * k;
                if (newRow < 0 || newRow >= boardSize || newCol < 0 || newCol >= boardSize)
                    break;
                if (board[newRow, newCol] == player)
                    count++;
                else if (board[newRow, newCol] == PlayerType.None)
                    empty++;
                else
                    break;
            }
            
            // Check in negative direction
            for (int k = 1; k < 3; k++)
            {
                int newRow = row - dir[0] * k;
                int newCol = col - dir[1] * k;
                if (newRow < 0 || newRow >= boardSize || newCol < 0 || newCol >= boardSize)
                    break;
                if (board[newRow, newCol] == player)
                    count++;
                else if (board[newRow, newCol] == PlayerType.None)
                    empty++;
                else
                    break;
            }
            
            // Score based on potential
            if (count >= 2)
                score += count * count * 10; // Favor longer sequences
            if (count >= 1 && empty >= 2)
                score += empty * 2; // Favor open positions
        }

        return score;
    }

    private bool IsEmptyBoard(PlayerType[,] board)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] != PlayerType.None)
                    return false;
            }
        }
        return true;
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
