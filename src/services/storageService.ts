import { PlayerStats } from '../types/GameTypes';

const API_BASE_URL = process.env.NODE_ENV === 'production' 
    ? 'https://potictac.azurewebsites.net/api' // Production API URL
    : 'http://localhost:5000/api'; // Local development API URL

class StorageService {
    private isOffline = false;
    private offlineStats = new Map<string, PlayerStats>();

    private async checkConnection(): Promise<boolean> {
        try {
            const response = await fetch(`${API_BASE_URL}/players`);
            this.isOffline = !response.ok;
            return !this.isOffline;
        } catch (error) {
            console.log("Running in offline mode - stats will not be persisted");
            this.isOffline = true;
            return false;
        }
    }

    async getPlayerStats(playerName: string): Promise<PlayerStats | null> {
        await this.checkConnection();

        if (this.isOffline) {
            return this.offlineStats.get(playerName) || null;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/players/${encodeURIComponent(playerName)}`);
            if (!response.ok) {
                if (response.status === 404) {
                    console.log("Player not found, will create new stats");
                    return null;
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const stats = await response.json();
            // Cache the stats for offline use
            this.offlineStats.set(playerName, stats);
            return stats;
        } catch (error) {
            console.error("Error fetching player stats:", error);
            // Return cached stats if available
            return this.offlineStats.get(playerName) || null;
        }
    }

    async savePlayerStats(playerName: string, stats: PlayerStats): Promise<void> {
        // Always update offline cache
        this.offlineStats.set(playerName, stats);

        if (this.isOffline) {
            console.log(`Stats saved locally for player: ${playerName} (offline mode)`);
            return;
        }

        try {
            const response = await fetch(`${API_BASE_URL}/players/${encodeURIComponent(playerName)}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(stats)
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            console.log(`Stats saved for player: ${playerName}`);
        } catch (error) {
            console.error("Error saving player stats:", error);
            this.isOffline = true;
            console.log("Switched to offline mode - stats will be saved locally only");
        }
    }

    async getAllPlayers(): Promise<{ name: string, stats: PlayerStats }[]> {
        if (this.isOffline) {
            return Array.from(this.offlineStats.entries()).map(([name, stats]) => ({
                name,
                stats
            }));
        }

        try {
            const response = await fetch(`${API_BASE_URL}/players`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const players = await response.json();
            // Update offline cache
            players.forEach((p: { name: string, stats: PlayerStats }) => {
                this.offlineStats.set(p.name, p.stats);
            });
            return players;
        } catch (error) {
            console.error("Error fetching players:", error);
            this.isOffline = true;
            // Return cached data
            return Array.from(this.offlineStats.entries()).map(([name, stats]) => ({
                name,
                stats
            }));
        }
    }

    async getLeaderboard(limit: number = 10): Promise<{ name: string, stats: PlayerStats }[]> {
        if (this.isOffline) {
            return Array.from(this.offlineStats.entries())
                .map(([name, stats]) => ({ name, stats }))
                .sort((a, b) => b.stats.winRate - a.stats.winRate)
                .slice(0, limit);
        }

        try {
            const response = await fetch(`${API_BASE_URL}/leaderboard?limit=${limit}`);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const leaderboard = await response.json();
            // Update offline cache
            leaderboard.forEach((p: { name: string, stats: PlayerStats }) => {
                this.offlineStats.set(p.name, p.stats);
            });
            return leaderboard;
        } catch (error) {
            console.error("Error fetching leaderboard:", error);
            this.isOffline = true;
            // Return cached leaderboard
            return Array.from(this.offlineStats.entries())
                .map(([name, stats]) => ({ name, stats }))
                .sort((a, b) => b.stats.winRate - a.stats.winRate)
                .slice(0, limit);
        }
    }

    isInOfflineMode(): boolean {
        return this.isOffline;
    }
}

// Export a singleton instance
export default new StorageService(); 