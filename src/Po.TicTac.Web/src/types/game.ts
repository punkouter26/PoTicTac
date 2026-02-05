/**
 * Game type definitions for PoTicTac
 * Mirrors the backend shared models for type safety
 */

/** Cell value representing X, O, or empty */
export enum CellValue {
  X = 'X',
  O = 'O',
  None = 'None',
}

/** Player role - Human or AI */
export enum PlayerRole {
  Human = 'Human',
  AI = 'AI',
}

/** 
 * Player type - maintained for backward compatibility
 * @deprecated Use CellValue for board cells, PlayerRole for player types
 */
export enum PlayerType {
  X = 'X',
  O = 'O',
  None = 'None',
  Human = 'Human',
  AI = 'AI',
}

/** Current game status */
export enum GameStatus {
  Playing = 'Playing',
  Won = 'Won',
  Draw = 'Draw',
}

/** AI difficulty levels */
export enum Difficulty {
  Easy = 'Easy',
  Medium = 'Medium',
  Hard = 'Hard',
}

/** Game mode selection */
export enum GameMode {
  Menu = 'Menu',
  Singleplayer = 'Singleplayer',
}

/** Statistics for a specific difficulty level */
export interface DifficultyStats {
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

/** Player statistics across all difficulties */
export interface PlayerStats {
  easy: DifficultyStats;
  medium: DifficultyStats;
  hard: DifficultyStats;
  totalWins: number;
  totalLosses: number;
  totalDraws: number;
  totalGames: number;
  overallWinRate: number;
}

/** Player statistics DTO from API */
export interface PlayerStatsDto {
  name: string;
  stats: PlayerStats;
}

/** Player configuration */
export interface Player {
  id: string;
  name: string;
  type: PlayerType;
  symbol: PlayerType;
  aiConfig?: AIConfig;
}

/** AI configuration */
export interface AIConfig {
  difficulty: Difficulty;
  thinkingTime?: number;
}

/** Move record */
export interface Move {
  player: PlayerType;
  position: [number, number];
  timestamp: number;
}

/** Last move info for highlighting */
export interface LastMoveInfo {
  row: number;
  col: number;
}

/** Game board state */
export interface GameBoardState {
  board: PlayerType[][];
  currentPlayer: PlayerType;
  gameStatus: GameStatus;
  winner: PlayerType | null;
  players: [Player, Player];
  moveHistory: Move[];
  winningCells: [number, number][] | null;
}

/** Board size constant - 6x6 grid */
export const BOARD_SIZE = 6;

/** Win condition - 4 in a row */
export const WIN_LENGTH = 4;

/** 
 * Creates an initial empty board 
 * Uses factory function pattern for immutability
 */
export function createEmptyBoard(): PlayerType[][] {
  return Array.from({ length: BOARD_SIZE }, () =>
    Array.from({ length: BOARD_SIZE }, () => PlayerType.None)
  );
}

/** Creates default difficulty stats */
export function createDefaultDifficultyStats(): DifficultyStats {
  return {
    wins: 0,
    losses: 0,
    draws: 0,
    totalGames: 0,
    winStreak: 0,
    currentStreak: 0,
    averageMovesPerGame: 0,
    totalMoves: 0,
    winRate: 0,
  };
}

/** Creates default player stats */
export function createDefaultPlayerStats(): PlayerStats {
  return {
    easy: createDefaultDifficultyStats(),
    medium: createDefaultDifficultyStats(),
    hard: createDefaultDifficultyStats(),
    totalWins: 0,
    totalLosses: 0,
    totalDraws: 0,
    totalGames: 0,
    overallWinRate: 0,
  };
}
