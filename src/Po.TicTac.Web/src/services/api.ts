import { PlayerStats, PlayerStatsDto, createDefaultPlayerStats } from '../types';

/**
 * API Service for communicating with the .NET backend
 * Implements resilience pattern - gracefully handles offline API
 */

// Base URL - uses Vite proxy in development
const API_BASE = '/api';

/**
 * Generic fetch wrapper with error handling
 */
async function fetchApi<T>(
  url: string,
  options?: RequestInit
): Promise<T | null> {
  try {
    const response = await fetch(`${API_BASE}${url}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...options?.headers,
      },
    });

    if (!response.ok) {
      console.warn(`API error: ${response.status} ${response.statusText}`);
      return null;
    }

    // Handle 204 No Content
    if (response.status === 204) {
      return null;
    }

    return await response.json();
  } catch (error) {
    console.warn('API request failed, app continues in offline mode:', error);
    return null;
  }
}

/**
 * Statistics API Service
 */
export const StatisticsApi = {
  /**
   * Get all player statistics
   */
  async getAllPlayerStatistics(): Promise<PlayerStatsDto[] | null> {
    return fetchApi<PlayerStatsDto[]>('/statistics');
  },

  /**
   * Get leaderboard with top players
   */
  async getLeaderboard(limit = 10): Promise<PlayerStatsDto[] | null> {
    return fetchApi<PlayerStatsDto[]>(`/statistics/leaderboard?limit=${limit}`);
  },

  /**
   * Get specific player stats
   */
  async getPlayerStats(playerName: string): Promise<PlayerStatsDto | null> {
    const result = await fetchApi<PlayerStatsDto>(
      `/players/${encodeURIComponent(playerName)}/stats`
    );
    
    // Return default stats if not found
    if (!result) {
      return {
        name: playerName,
        stats: createDefaultPlayerStats(),
      };
    }
    
    return result;
  },

  /**
   * Save player stats to backend
   */
  async savePlayerStats(
    playerName: string,
    stats: PlayerStats
  ): Promise<boolean> {
    try {
      const response = await fetch(
        `${API_BASE}/players/${encodeURIComponent(playerName)}/stats`,
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(stats),
        }
      );
      return response.ok;
    } catch (error) {
      console.warn('Failed to save player stats:', error);
      return false;
    }
  },
};

/**
 * Health check API
 */
export const HealthApi = {
  /**
   * Check if the API is available
   */
  async checkHealth(): Promise<boolean> {
    try {
      const response = await fetch('/health', { method: 'GET' });
      return response.ok;
    } catch {
      return false;
    }
  },
};
