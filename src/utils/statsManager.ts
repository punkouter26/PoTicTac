import { Player, PlayerStats, GameState } from '../types/GameTypes';
import { GameStats, HeatMapData, PerformanceAnalytics } from '../types/GameStats';

export const createInitialStats = (): GameStats => ({
    totalGames: 0,
    wins: 0,
    losses: 0,
    draws: 0,
    winRate: 0,
    winStreak: 0,
    currentStreak: 0,
    averageMovesPerGame: 0,
    totalMoves: 0,
    averageGameLength: 0,
    favoriteMoves: [],
    winningPatterns: [],
    performanceHistory: []
});

export const updatePlayerStats = (
    currentStats: GameStats,
    gameState: GameState,
    gameLength: number,
    playerSymbol: number
): GameStats => {
    const newStats = { ...currentStats };
    newStats.totalGames++;
    newStats.totalMoves += gameLength;
    newStats.averageMovesPerGame = newStats.totalMoves / newStats.totalGames;
    
    // Update win/loss/draw counts and streaks
    if (gameState.gameStatus === 'won') {
        if (gameState.winner === playerSymbol) {
            newStats.wins++;
            newStats.currentStreak++;
            newStats.winStreak = Math.max(newStats.winStreak, newStats.currentStreak);
        } else {
            newStats.losses++;
            newStats.currentStreak = 0;
        }
    } else if (gameState.gameStatus === 'draw') {
        newStats.draws++;
        newStats.currentStreak = 0;
    }

    // Calculate win rate
    newStats.winRate = (newStats.wins / newStats.totalGames) * 100;

    // Update average game length
    newStats.averageGameLength = 
        ((newStats.averageGameLength * (newStats.totalGames - 1)) + gameLength) / newStats.totalGames;

    // Add to performance history
    newStats.performanceHistory.push({
        date: new Date().toISOString(),
        result: gameState.gameStatus === 'won' 
            ? (gameState.winner === playerSymbol ? 'win' : 'loss')
            : 'draw',
        gameLength
    });

    return newStats;
};

export const generateHeatMapData = (gameState: GameState, playerSymbol: number): HeatMapData[] => {
    const heatMap: HeatMapData[] = [];
    const board = gameState.board;

    for (let row = 0; row < 3; row++) {
        for (let col = 0; col < 3; col++) {
            if (board[row][col] === playerSymbol) {
                heatMap.push({
                    position: `${row},${col}`,
                    value: 1,
                    type: gameState.gameStatus === 'won' && gameState.winner === playerSymbol ? 'win' : 'loss'
                });
            }
        }
    }

    return heatMap;
};

export const analyzePerformance = (stats: GameStats): PerformanceAnalytics => {
    const analytics: PerformanceAnalytics = {
        strengths: [],
        weaknesses: [],
        improvementSuggestions: [],
        skillLevel: 'beginner'
    };

    // Analyze win rate for skill level
    if (stats.winRate >= 80) {
        analytics.skillLevel = 'expert';
    } else if (stats.winRate >= 60) {
        analytics.skillLevel = 'advanced';
    } else if (stats.winRate >= 40) {
        analytics.skillLevel = 'intermediate';
    }

    // Generate suggestions based on performance
    if (stats.wins < stats.losses) {
        analytics.weaknesses.push('Defensive play');
        analytics.improvementSuggestions.push('Focus on blocking opponent\'s winning moves');
    }

    if (stats.averageGameLength < 5) {
        analytics.weaknesses.push('Game length');
        analytics.improvementSuggestions.push('Try to plan moves ahead and think strategically');
    }

    if (stats.winRate > 50) {
        analytics.strengths.push('Overall performance');
    }

    return analytics;
};

// Local storage management
const STATS_STORAGE_KEY = 'tictactoe_player_stats';

export const saveStatsToLocalStorage = (players: [Player, Player]) => {
    try {
        localStorage.setItem(STATS_STORAGE_KEY, JSON.stringify(players));
    } catch (error) {
        console.error('Failed to save stats to localStorage:', error);
    }
};

export const loadStatsFromLocalStorage = (): [Player, Player] | null => {
    try {
        const savedStats = localStorage.getItem(STATS_STORAGE_KEY);
        if (savedStats) {
            return JSON.parse(savedStats);
        }
    } catch (error) {
        console.error('Failed to load stats from localStorage:', error);
    }
    return null; 
} 