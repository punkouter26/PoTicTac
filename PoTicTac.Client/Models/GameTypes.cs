namespace PoTicTac.Client.Models
{
    public enum GameStatus
    {
        Playing,
        Won,
        Draw
    }

    public enum PlayerType
    {
        Human,
        AI
    }

    public enum GameMode
    {
        Singleplayer,
        Multiplayer,
        Tournament,
        Practice
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class PlayerStats
    {
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public double WinRate { get; set; }
        public int WinStreak { get; set; }
        public int CurrentStreak { get; set; }
        public double AverageMovesPerGame { get; set; }
        public int TotalMoves { get; set; }
        public double AverageGameLength { get; set; }
        public List<FavoriteMove> FavoriteMoves { get; set; } = new();
        public List<WinningPattern> WinningPatterns { get; set; } = new();
        public List<PerformanceRecord> PerformanceHistory { get; set; } = new();
    }

    public class FavoriteMove
    {
        public string Position { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class WinningPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PerformanceRecord
    {
        public DateTime Date { get; set; }
        public string Result { get; set; } = string.Empty; // "win", "loss", "draw"
        public int GameLength { get; set; }
    }

    public class Player
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PlayerType Type { get; set; }
        public int Symbol { get; set; } // 1 or 2
        public AIConfig? AiConfig { get; set; }
        public PlayerStats Stats { get; set; } = new();
    }

    public class Move
    {
        public int Player { get; set; } // 1 or 2
        public int[] Position { get; set; } = new int[2]; // [row, col]
        public long Timestamp { get; set; }
    }

    public class GameState
    {
        public int[][] Board { get; set; } = Array.Empty<int[]>();
        public int CurrentPlayer { get; set; } // 1 or 2
        public GameStatus GameStatus { get; set; }
        public int? Winner { get; set; } // 1, 2, or null
        public Player[] Players { get; set; } = new Player[2];
        public List<Move> MoveHistory { get; set; } = new();
        public List<Move> UndoStack { get; set; } = new();
        public List<Move> RedoStack { get; set; } = new();
        public List<int[]>? WinningCells { get; set; }
        public SessionStats SessionStats { get; set; } = new();
    }

    public class SessionStats
    {
        public int TotalGames { get; set; }
        public long StartTime { get; set; }
        public long LastUpdateTime { get; set; }
    }

    public class AIConfig
    {
        public Difficulty Difficulty { get; set; }
        public int? ThinkingTime { get; set; }
    }

    public class TournamentSettings
    {
        public int Rounds { get; set; }
        public int TimeLimit { get; set; }
        public int WinningScore { get; set; }
    }

    public class MultiplayerGameInfo
    {
        public string GameId { get; set; } = string.Empty;
        public bool IsHost { get; set; }
    }

    public class GameScore
    {
        public int Player { get; set; }
        public int AI { get; set; }
    }
}
