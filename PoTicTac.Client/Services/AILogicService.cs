using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services
{
    public class AILogicService
    {
        private readonly GameLogicService _gameLogic;
        private readonly Random _random;

        public AILogicService(GameLogicService gameLogic)
        {
            _gameLogic = gameLogic;
            _random = new Random();
        }

        public async Task<int[]> GetAiMove(GameState gameState, Difficulty difficulty)
        {
            await Task.Delay(100); // Simulate thinking time

            return difficulty switch
            {
                Difficulty.Easy => GetEasyMove(gameState),
                Difficulty.Medium => GetMediumMove(gameState),
                Difficulty.Hard => GetHardMove(gameState),
                _ => GetMediumMove(gameState)
            };
        }

        private int[] GetEasyMove(GameState gameState)
        {
            // Easy AI: Random valid moves with slight preference for center
            var availableMoves = GetAvailableMoves(gameState.Board);
            
            if (availableMoves.Count == 0)
                return new[] { 0, 0 }; // Fallback

            // 30% chance to play optimally, 70% random
            if (_random.NextDouble() < 0.3)
            {
                return GetMediumMove(gameState);
            }

            return availableMoves[_random.Next(availableMoves.Count)];
        }

        private int[] GetMediumMove(GameState gameState)
        {
            var aiPlayer = gameState.CurrentPlayer;
            var humanPlayer = aiPlayer == 1 ? 2 : 1;

            // 1. Check if AI can win
            var winMove = FindWinningMove(gameState.Board, aiPlayer);
            if (winMove != null)
                return winMove;

            // 2. Block human from winning
            var blockMove = FindWinningMove(gameState.Board, humanPlayer);
            if (blockMove != null)
                return blockMove;

            // 3. Look for moves that create multiple threats
            var threatMove = FindThreatMove(gameState.Board, aiPlayer);
            if (threatMove != null)
                return threatMove;

            // 4. Play center or strategic positions
            var strategicMove = GetStrategicMove(gameState.Board);
            if (strategicMove != null)
                return strategicMove;

            // 5. Random move as fallback
            var availableMoves = GetAvailableMoves(gameState.Board);
            return availableMoves.Count > 0 ? availableMoves[_random.Next(availableMoves.Count)] : new[] { 0, 0 };
        }

        private int[] GetHardMove(GameState gameState)
        {
            // Hard AI uses minimax with alpha-beta pruning
            var bestMove = MinimaxWithAlphaBeta(gameState, 4, int.MinValue, int.MaxValue, true);
            return bestMove.Item2 ?? GetMediumMove(gameState);
        }

        private (int score, int[]? move) MinimaxWithAlphaBeta(GameState gameState, int depth, int alpha, int beta, bool isMaximizing)
        {
            var aiPlayer = gameState.Players.First(p => p.Type == PlayerType.AI).Symbol;
            var humanPlayer = aiPlayer == 1 ? 2 : 1;

            // Terminal conditions
            if (depth == 0 || gameState.GameStatus != GameStatus.Playing)
            {
                return (EvaluatePosition(gameState, aiPlayer), null);
            }

            var availableMoves = GetAvailableMoves(gameState.Board);
            if (availableMoves.Count == 0)
            {
                return (0, null); // Draw
            }

            int[]? bestMove = null;

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                foreach (var move in availableMoves)
                {
                    var newState = _gameLogic.MakeMove(gameState, move[0], move[1]);
                    var eval = MinimaxWithAlphaBeta(newState, depth - 1, alpha, beta, false).score;
                    
                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        bestMove = move;
                    }
                    
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break; // Alpha-beta pruning
                }
                return (maxEval, bestMove);
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in availableMoves)
                {
                    var newState = _gameLogic.MakeMove(gameState, move[0], move[1]);
                    var eval = MinimaxWithAlphaBeta(newState, depth - 1, alpha, beta, true).score;
                    
                    if (eval < minEval)
                    {
                        minEval = eval;
                        bestMove = move;
                    }
                    
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break; // Alpha-beta pruning
                }
                return (minEval, bestMove);
            }
        }

        private int EvaluatePosition(GameState gameState, int aiPlayer)
        {
            if (gameState.GameStatus == GameStatus.Won)
            {
                return gameState.Winner == aiPlayer ? 1000 : -1000;
            }
            if (gameState.GameStatus == GameStatus.Draw)
            {
                return 0;
            }

            int score = 0;
            var board = gameState.Board;

            // Evaluate all possible lines
            for (int row = 0; row < GameLogicService.BOARD_SIZE; row++)
            {
                for (int col = 0; col < GameLogicService.BOARD_SIZE; col++)
                {
                    score += EvaluatePosition(board, row, col, aiPlayer);
                }
            }

            return score;
        }

        private int EvaluatePosition(int[][] board, int row, int col, int aiPlayer)
        {
            int score = 0;
            var directions = new int[][]
            {
                new[] { 0, 1 },  // horizontal
                new[] { 1, 0 },  // vertical
                new[] { 1, 1 },  // diagonal \
                new[] { 1, -1 }  // diagonal /
            };

            foreach (var direction in directions)
            {
                score += EvaluateLine(board, row, col, direction[0], direction[1], aiPlayer);
            }

            return score;
        }

        private int EvaluateLine(int[][] board, int row, int col, int dx, int dy, int aiPlayer)
        {
            int aiCount = 0;
            int humanCount = 0;
            int emptyCount = 0;

            for (int i = 0; i < GameLogicService.WIN_LENGTH; i++)
            {
                int newRow = row + dx * i;
                int newCol = col + dy * i;

                if (!_gameLogic.IsValidPosition(newRow, newCol))
                    return 0;

                int cell = board[newRow][newCol];
                if (cell == aiPlayer)
                    aiCount++;
                else if (cell != 0)
                    humanCount++;
                else
                    emptyCount++;
            }

            // If both players have pieces in this line, it's not useful
            if (aiCount > 0 && humanCount > 0)
                return 0;

            // Score based on AI pieces in line
            if (aiCount > 0)
            {
                return (int)Math.Pow(10, aiCount);
            }

            // Negative score for human pieces
            if (humanCount > 0)
            {
                return -(int)Math.Pow(10, humanCount);
            }

            return 0;
        }

        private List<int[]> GetAvailableMoves(int[][] board)
        {
            var moves = new List<int[]>();
            for (int row = 0; row < GameLogicService.BOARD_SIZE; row++)
            {
                for (int col = 0; col < GameLogicService.BOARD_SIZE; col++)
                {
                    if (board[row][col] == 0)
                    {
                        moves.Add(new[] { row, col });
                    }
                }
            }
            return moves;
        }

        private int[]? FindWinningMove(int[][] board, int player)
        {
            var availableMoves = GetAvailableMoves(board);
            
            foreach (var move in availableMoves)
            {
                // Temporarily make the move
                board[move[0]][move[1]] = player;
                
                // Check if this creates a win
                var winningCells = _gameLogic.CheckWinner(board, move);
                
                // Undo the move
                board[move[0]][move[1]] = 0;
                
                if (winningCells != null)
                    return move;
            }
            
            return null;
        }

        private int[]? FindThreatMove(int[][] board, int player)
        {
            var availableMoves = GetAvailableMoves(board);
            
            foreach (var move in availableMoves)
            {
                // Temporarily make the move
                board[move[0]][move[1]] = player;
                
                // Count how many winning moves this creates for next turn
                int threatCount = 0;
                var nextMoves = GetAvailableMoves(board);
                
                foreach (var nextMove in nextMoves)
                {
                    board[nextMove[0]][nextMove[1]] = player;
                    if (_gameLogic.CheckWinner(board, nextMove) != null)
                        threatCount++;
                    board[nextMove[0]][nextMove[1]] = 0;
                }
                
                // Undo the move
                board[move[0]][move[1]] = 0;
                
                // If this creates multiple threats, it's a good move
                if (threatCount >= 2)
                    return move;
            }
            
            return null;
        }

        private int[]? GetStrategicMove(int[][] board)
        {
            var center = GameLogicService.BOARD_SIZE / 2;
            
            // Prefer center positions
            var centerPositions = new[]
            {
                new[] { center, center },
                new[] { center - 1, center },
                new[] { center, center - 1 },
                new[] { center - 1, center - 1 }
            };

            foreach (var pos in centerPositions)
            {
                if (_gameLogic.IsValidPosition(pos[0], pos[1]) && board[pos[0]][pos[1]] == 0)
                    return pos;
            }

            return null;
        }
    }
}
