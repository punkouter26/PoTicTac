export interface GameStats {
    totalGames: number;
    wins: number;
    losses: number;
    draws: number;
    winRate: number;
    winStreak: number;
    currentStreak: number;
    averageMovesPerGame: number;
    totalMoves: number;
    averageGameLength: number;
    favoriteMoves: Array<{
        position: string;
        count: number;
    }>;
    winningPatterns: Array<{
        pattern: string;
        count: number;
    }>;
    performanceHistory: Array<{
        date: string;
        result: 'win' | 'loss' | 'draw';
        gameLength: number;
    }>;
}

export interface HeatMapData {
    position: string;
    value: number;
    type: 'win' | 'loss' | 'draw';
}

export interface PerformanceAnalytics {
    strengths: string[];
    weaknesses: string[];
    improvementSuggestions: string[];
    skillLevel: 'beginner' | 'intermediate' | 'advanced' | 'expert';
} 