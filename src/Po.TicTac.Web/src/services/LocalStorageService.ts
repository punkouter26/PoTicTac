import { PlayerStats, createDefaultPlayerStats } from '../types';

/**
 * Local Storage Service
 * Provides client-side persistence for offline functionality
 * Implements Facade pattern for localStorage operations
 */

const STORAGE_KEYS = {
  PLAYER_NAME: 'potictac_playerName',
  PLAYER_STATS: 'potictac_playerStats',
  DIFFICULTY: 'potictac_difficulty',
} as const;

export const LocalStorageService = {
  /**
   * Get player name from local storage
   */
  getPlayerName(): string {
    try {
      return localStorage.getItem(STORAGE_KEYS.PLAYER_NAME) ?? '';
    } catch {
      return '';
    }
  },

  /**
   * Save player name to local storage
   */
  setPlayerName(name: string): void {
    try {
      localStorage.setItem(STORAGE_KEYS.PLAYER_NAME, name);
    } catch (error) {
      console.warn('Failed to save player name:', error);
    }
  },

  /**
   * Get player stats from local storage (offline cache)
   */
  getPlayerStats(playerName: string): PlayerStats {
    try {
      const key = `${STORAGE_KEYS.PLAYER_STATS}_${playerName}`;
      const stored = localStorage.getItem(key);
      if (stored) {
        return JSON.parse(stored) as PlayerStats;
      }
    } catch (error) {
      console.warn('Failed to read player stats:', error);
    }
    return createDefaultPlayerStats();
  },

  /**
   * Save player stats to local storage (offline cache)
   */
  setPlayerStats(playerName: string, stats: PlayerStats): void {
    try {
      const key = `${STORAGE_KEYS.PLAYER_STATS}_${playerName}`;
      localStorage.setItem(key, JSON.stringify(stats));
    } catch (error) {
      console.warn('Failed to save player stats:', error);
    }
  },

  /**
   * Get preferred difficulty
   */
  getDifficulty(): string {
    try {
      return localStorage.getItem(STORAGE_KEYS.DIFFICULTY) ?? 'Medium';
    } catch {
      return 'Medium';
    }
  },

  /**
   * Save preferred difficulty
   */
  setDifficulty(difficulty: string): void {
    try {
      localStorage.setItem(STORAGE_KEYS.DIFFICULTY, difficulty);
    } catch (error) {
      console.warn('Failed to save difficulty:', error);
    }
  },
};
