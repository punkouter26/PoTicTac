import { Player, PlayerStats, GameState } from '../types/GameTypes';

export const createInitialStats = (): PlayerStats => ({
    wins: 0,
    losses: 0,
    draws: 0,
    totalGames: 0,
    winStreak: 0,
    currentStreak: 0,
    averageMovesPerGame: 0,
    totalMoves: 0,
    winRate: 0
});

export const updatePlayerStats = (
    player: Player,
    gameState: GameState,
    isWinner: boolean,
    isDraw: boolean
): Player => {
    const stats = { ...player.stats };
    const movesThisGame = gameState.moveHistory.filter(move => move.player === player.symbol).length;

    // Update basic stats
    stats.totalGames += 1;
    stats.totalMoves += movesThisGame;
    stats.averageMovesPerGame = stats.totalMoves / stats.totalGames;

    if (isWinner) {
        stats.wins += 1;
        stats.currentStreak += 1;
        stats.winStreak = Math.max(stats.winStreak, stats.currentStreak);
    } else if (isDraw) {
        stats.draws += 1;
        stats.currentStreak = 0;
    } else {
        stats.losses += 1;
        stats.currentStreak = 0;
    }

    // Calculate win rate
    stats.winRate = stats.wins / stats.totalGames;

    return {
        ...player,
        stats
    };
};

export const getPlayerAchievements = (stats: PlayerStats): string[] => {
    const achievements: string[] = [];

    if (stats.currentStreak >= 3) {
        achievements.push('🔥 Hot Streak');
    }
    if (stats.winRate >= 0.7 && stats.totalGames >= 5) {
        achievements.push('👑 Dominating');
    }
    if (stats.totalGames >= 10) {
        achievements.push('🎮 Veteran');
    }
    if (stats.draws === 0 && stats.totalGames >= 5) {
        achievements.push('⚔️ Decisive');
    }
    if (stats.averageMovesPerGame <= 4 && stats.totalGames >= 5) {
        achievements.push('⚡ Quick Winner');
    }

    return achievements;
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
}; 