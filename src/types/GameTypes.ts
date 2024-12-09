export type GameStatus = 'playing' | 'won' | 'draw';
export type PlayerType = 'human' | 'ai';

export interface Player {
    id: string;
    type: PlayerType;
    symbol: 1 | 2;
    aiConfig?: AIConfig;
    name?: string;
}

export interface GameState {
    board: number[][];
    currentPlayer: 1 | 2;
    gameStatus: GameStatus;
    winner: number | null;
    winningCells?: Array<[number, number]>;
    players: [Player, Player];
    moveHistory?: Array<{
        player: number;
        position: [number, number];
        timestamp: number;
    }>;
}

export interface AIConfig {
    difficulty: 'easy' | 'medium' | 'hard';
    thinkingTime?: number;
}
