import React from 'react';
import { GameStats, PerformanceAnalytics } from '../types/GameStats';
import './Statistics.css';

interface StatisticsProps {
    stats: GameStats;
    analytics: PerformanceAnalytics;
}

export const Statistics: React.FC<StatisticsProps> = ({ stats, analytics }) => {
    return (
        <div className="statistics-container">
            <div className="stats-grid">
                <div className="stat-card">
                    <h3>Overall Performance</h3>
                    <div className="stat-value">{stats.winRate.toFixed(1)}%</div>
                    <div className="stat-label">Win Rate</div>
                </div>
                
                <div className="stat-card">
                    <h3>Games Played</h3>
                    <div className="stat-value">{stats.totalGames}</div>
                    <div className="stat-label">Total</div>
                </div>
                
                <div className="stat-card">
                    <h3>Average Game Length</h3>
                    <div className="stat-value">{stats.averageGameLength.toFixed(1)}</div>
                    <div className="stat-label">Moves</div>
                </div>
            </div>

            <div className="analytics-section">
                <h3>Performance Analysis</h3>
                <div className="skill-level">
                    <span className="level-badge">{analytics.skillLevel}</span>
                </div>
                
                <div className="strengths-weaknesses">
                    <div className="strengths">
                        <h4>Strengths</h4>
                        <ul>
                            {analytics.strengths.map((strength, index) => (
                                <li key={index}>✅ {strength}</li>
                            ))}
                        </ul>
                    </div>
                    
                    <div className="weaknesses">
                        <h4>Areas for Improvement</h4>
                        <ul>
                            {analytics.weaknesses.map((weakness, index) => (
                                <li key={index}>⚠️ {weakness}</li>
                            ))}
                        </ul>
                    </div>
                </div>

                <div className="suggestions">
                    <h4>Improvement Suggestions</h4>
                    <ul>
                        {analytics.improvementSuggestions.map((suggestion, index) => (
                            <li key={index}>💡 {suggestion}</li>
                        ))}
                    </ul>
                </div>
            </div>

            <div className="performance-history">
                <h3>Recent Performance</h3>
                <div className="history-chart">
                    {stats.performanceHistory.slice(-10).map((game, index) => (
                        <div 
                            key={index} 
                            className={`history-bar ${game.result}`}
                            style={{ height: `${(game.gameLength / 9) * 100}%` }}
                            title={`${game.result.toUpperCase()} - ${game.gameLength} moves`}
                        />
                    ))}
                </div>
            </div>
        </div>
    );
}; 