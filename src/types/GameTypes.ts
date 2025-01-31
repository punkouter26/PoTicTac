export type GameStatus = 'playing' | 'won' | 'draw';
export type PlayerType = 'human' | 'ai';
export type GameMode = 'singleplayer' | 'multiplayer' | 'tournament' | 'practice';

export interface PlayerStats {
    wins: number;
    losses: number;
    draws: number;
    totalGames: number;
    winStreak: number;
    currentStreak: number;
    averageMovesPerGame: number;
    totalMoves: number;
    winRate: number;
}

export interface Player {
    id: string;
    name: string;
    type: PlayerType;
    symbol: 1 | 2;
    aiConfig?: AIConfig;
    stats: PlayerStats;
}

export interface Move {
    player: 1 | 2;
    position: [number, number];
    timestamp: number;
}

export interface GameState {
    board: number[][];
    currentPlayer: 1 | 2;
    gameStatus: GameStatus;
    winner: 1 | 2 | null;
    players: [Player, Player];
    moveHistory: Move[];
    undoStack: Move[];
    redoStack: Move[];
    winningCells?: Array<[number, number]>;
    sessionStats: {
        totalGames: number;
        startTime: number;
        lastUpdateTime: number;
    };
}

export interface AIConfig {
    difficulty: 'easy' | 'medium' | 'hard';
    thinkingTime?: number;
}

export interface TournamentSettings {
    rounds: number;
    timeLimit: number;
    winningScore: number;
}
