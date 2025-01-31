import React from 'react';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
    ArcElement,
    PointElement,
    LineElement
} from 'chart.js';
import { Bar, Pie, Line } from 'react-chartjs-2';
import { Player, PlayerStats } from '../types/GameTypes';

// Register ChartJS components
ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
    ArcElement,
    PointElement,
    LineElement
);

interface DetailedStatsProps {
    player: Player;
    onClose: () => void;
}

const DetailedStats: React.FC<DetailedStatsProps> = ({ player, onClose }) => {
    const stats = player.stats;
    const playerColor = player.symbol === 1 ? '#ff4757' : '#2ed573';

    // Prepare data for Win/Loss/Draw pie chart
    const resultDistributionData = {
        labels: ['Wins', 'Losses', 'Draws'],
        datasets: [
            {
                data: [stats.wins, stats.losses, stats.draws],
                backgroundColor: [
                    'rgba(46, 213, 115, 0.8)',
                    'rgba(255, 71, 87, 0.8)',
                    'rgba(47, 53, 66, 0.8)'
                ],
                borderColor: [
                    'rgba(46, 213, 115, 1)',
                    'rgba(255, 71, 87, 1)',
                    'rgba(47, 53, 66, 1)'
                ],
                borderWidth: 1
            }
        ]
    };

    // Prepare data for performance metrics bar chart
    const performanceData = {
        labels: ['Win Rate', 'Current Streak', 'Best Streak', 'Avg Moves'],
        datasets: [
            {
                label: 'Performance Metrics',
                data: [
                    stats.winRate * 100,
                    stats.currentStreak,
                    stats.winStreak,
                    stats.averageMovesPerGame
                ],
                backgroundColor: playerColor + '80',
                borderColor: playerColor,
                borderWidth: 1
            }
        ]
    };

    // Mock data for game history line chart (in a real app, you'd store historical data)
    const gameHistory = {
        labels: Array.from({ length: stats.totalGames }, (_, i) => `Game ${i + 1}`),
        datasets: [
            {
                label: 'Moves per Game',
                data: Array.from({ length: stats.totalGames }, () => 
                    stats.averageMovesPerGame + (Math.random() * 2 - 1)
                ),
                borderColor: playerColor,
                backgroundColor: playerColor + '20',
                fill: true,
                tension: 0.4
            }
        ]
    };

    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom' as const
            }
        }
    };

    return (
        <div style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            zIndex: 1000
        }}>
            <div style={{
                backgroundColor: '#fff',
                borderRadius: '16px',
                padding: '24px',
                width: '90%',
                maxWidth: '1000px',
                maxHeight: '90vh',
                overflowY: 'auto',
                position: 'relative'
            }}>
                <button
                    onClick={onClose}
                    style={{
                        position: 'absolute',
                        top: '16px',
                        right: '16px',
                        background: 'none',
                        border: 'none',
                        fontSize: '24px',
                        cursor: 'pointer',
                        color: '#2f3542'
                    }}
                >
                    ×
                </button>

                <h2 style={{
                    color: playerColor,
                    marginBottom: '24px',
                    textAlign: 'center'
                }}>
                    {player.name}'s Detailed Statistics
                </h2>

                <div style={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
                    gap: '24px',
                    marginBottom: '24px'
                }}>
                    {/* Game Results Distribution */}
                    <div style={{
                        backgroundColor: '#f8f9fa',
                        padding: '16px',
                        borderRadius: '12px',
                        height: '300px'
                    }}>
                        <h3 style={{ textAlign: 'center', marginBottom: '16px' }}>
                            Game Results Distribution
                        </h3>
                        <Pie data={resultDistributionData} options={chartOptions} />
                    </div>

                    {/* Performance Metrics */}
                    <div style={{
                        backgroundColor: '#f8f9fa',
                        padding: '16px',
                        borderRadius: '12px',
                        height: '300px'
                    }}>
                        <h3 style={{ textAlign: 'center', marginBottom: '16px' }}>
                            Performance Metrics
                        </h3>
                        <Bar data={performanceData} options={chartOptions} />
                    </div>
                </div>

                {/* Game History */}
                <div style={{
                    backgroundColor: '#f8f9fa',
                    padding: '16px',
                    borderRadius: '12px',
                    height: '300px',
                    marginBottom: '24px'
                }}>
                    <h3 style={{ textAlign: 'center', marginBottom: '16px' }}>
                        Game History
                    </h3>
                    <Line data={gameHistory} options={chartOptions} />
                </div>

                {/* Additional Stats */}
                <div style={{
                    display: 'grid',
                    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                    gap: '16px',
                    backgroundColor: '#f8f9fa',
                    padding: '16px',
                    borderRadius: '12px'
                }}>
                    <div>
                        <h4>Efficiency</h4>
                        <p>Average moves per game: {stats.averageMovesPerGame.toFixed(1)}</p>
                        <p>Total moves: {stats.totalMoves}</p>
                    </div>
                    <div>
                        <h4>Achievements</h4>
                        <p>Best win streak: {stats.winStreak}</p>
                        <p>Current streak: {stats.currentStreak}</p>
                    </div>
                    <div>
                        <h4>Overall</h4>
                        <p>Win rate: {(stats.winRate * 100).toFixed(1)}%</p>
                        <p>Total games: {stats.totalGames}</p>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default DetailedStats; 