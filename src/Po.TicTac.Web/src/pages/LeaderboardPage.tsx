import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { StatisticsApi } from '../services';
import { PlayerStatsDto } from '../types';
import '../styles/pages.css';

/**
 * Leaderboard Page - displays top players
 */
export function LeaderboardPage() {
  const navigate = useNavigate();
  const [leaderboard, setLeaderboard] = useState<PlayerStatsDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchLeaderboard() {
      try {
        const data = await StatisticsApi.getLeaderboard(10);
        if (data) {
          setLeaderboard(data);
        } else {
          setError('Unable to load leaderboard. API may be offline.');
        }
      } catch (err) {
        setError('Failed to load leaderboard');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    fetchLeaderboard();
  }, []);

  const formatPercentage = (value: number, totalGames: number): string => {
    // Issue 5 fix: Show N/A for players with no games
    if (totalGames === 0) return 'N/A';
    return `${(value * 100).toFixed(1)}%`;
  };

  const topThree = leaderboard.slice(0, 3);
  const restOfPlayers = leaderboard.slice(3);

  return (
    <div className="leaderboard-page">
      <h1>🏆 Leaderboard</h1>

      {loading && (
        <div className="leaderboard-skeleton">
           <div className="podium-skeleton">
             <div className="skeleton h-20 w-24 delay-1"></div>
             <div className="skeleton h-32 w-28 delay-0"></div>
             <div className="skeleton h-16 w-24 delay-2"></div>
           </div>
           {[1, 2, 3, 4, 5].map(i => (
             <div key={i} className="skeleton h-16 w-full mb-2" style={{opacity: 1 - i*0.1}}></div>
           ))}
        </div>
      )}

      {error && (
        <div className="error-state glass-panel">
          <div className="error-icon">⚠️</div>
          <h3>Connection Lost</h3>
          <p>{error}</p>
          <button className="retry-button" onClick={() => window.location.reload()}>Retry</button>
        </div>
      )}

      {!loading && !error && leaderboard.length === 0 && (
        <div className="empty-state glass-panel">
          <div className="empty-icon">🏆</div>
          <h3>No Legends Yet</h3>
          <p>Be the first to claim victory!</p>
        </div>
      )}

      {!loading && !error && leaderboard.length > 0 && (
        <>
          {/* Podium for Top 3 */}
          <div className="podium-container">
            {topThree[1] && (
              <div className="podium-item rank-2 glass-card">
                <div className="podium-avatar">🥈</div>
                <div className="podium-name">{topThree[1].name}</div>
                <div className="podium-stats">{topThree[1].stats.totalWins} Wins</div>
                <div className="podium-bar"></div>
              </div>
            )}
            
            {topThree[0] && (
              <div className="podium-item rank-1 glass-card">
                <div className="podium-crown">👑</div>
                <div className="podium-avatar">🥇</div>
                <div className="podium-name">{topThree[0].name}</div>
                <div className="podium-stats">{topThree[0].stats.totalWins} Wins</div>
                <div className="podium-bar"></div>
              </div>
            )}

            {topThree[2] && (
              <div className="podium-item rank-3 glass-card">
                <div className="podium-avatar">🥉</div>
                <div className="podium-name">{topThree[2].name}</div>
                <div className="podium-stats">{topThree[2].stats.totalWins} Wins</div>
                <div className="podium-bar"></div>
              </div>
            )}
          </div>

          <div className="leaderboard-table glass-panel">
            <div className="table-header">
              <span className="rank">#</span>
              <span className="player">Player</span>
              <span className="wins">Wins</span>
              <span className="games">Km</span>
              <span className="winrate">Rate</span>
            </div>
            {restOfPlayers.map((player, index) => (
              <div key={player.name} className="table-row">
                <span className="rank is-mono">{index + 4}</span>
                <span className="player">{player.name}</span>
                <span className="wins">{player.stats.totalWins}</span>
                <span className="games">{player.stats.totalGames}</span>
                <span className="winrate">
                  {formatPercentage(player.stats.overallWinRate, player.stats.totalGames)}
                </span>
              </div>
            ))}
            {restOfPlayers.length === 0 && leaderboard.length <= 3 && (
               <div className="table-footer-msg">Play more to fill the ranks!</div>
            )}
          </div>
        </>
      )}

      {/* Unified HUD Navigation */}
      <nav className="hud-nav glass-panel">
        <div className="hud-content">
          <div className="hud-status">
            <span className="status-dot online"></span> Online
          </div>
          <div className="hud-controls">
             <button className="hud-btn active" title="Leaderboard">🏆</button>
             <button className="hud-btn" onClick={() => navigate('/')} title="Play Game">🎮 Play</button>
             <button className="hud-btn" title="Settings">⚙️</button>
          </div>
        </div>
      </nav>
    </div>
  );
}
