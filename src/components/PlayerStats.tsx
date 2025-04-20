import React, { useState } from 'react';
import { Player } from '../types/GameTypes';
import DetailedStats from './DetailedStats';
import '../styles/animations.css';

interface PlayerStatsProps {
    player: Player;
    onNameChange: (name: string) => void;
}

const StatItem: React.FC<{ label: string; value: string | number }> = ({ label, value }) => (
    <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        padding: '4px 0',
        borderBottom: '1px solid rgba(0,0,0,0.1)'
    }}>
        <span style={{ color: '#666', fontSize: '0.9rem' }}>{label}:</span>
        <span style={{ fontWeight: 'bold', color: '#2f3542' }}>{value}</span>
    </div>
);

const PlayerStats: React.FC<PlayerStatsProps> = ({ player, onNameChange }) => {
    const [showDetailedStats, setShowDetailedStats] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [tempName, setTempName] = useState(player.name);

    const {
        wins,
        losses,
        draws,
        totalGames,
        winStreak,
        currentStreak,
        averageMovesPerGame,
        winRate
    } = player.stats;

    const handleNameSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (tempName.trim()) {
            onNameChange(tempName.trim());
            setIsEditing(false);
        }
    };

    return (
        <>
            <div style={{
                backgroundColor: '#fff',
                borderRadius: '12px',
                padding: '16px',
                boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                width: '200px',
                transition: 'all 0.3s ease'
            }}>
                <div style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px',
                    marginBottom: '12px',
                    padding: '8px',
                    backgroundColor: player.symbol === 1 ? '#ff475720' : '#2ed57320',
                    borderRadius: '8px'
                }}>
                    <span style={{
                        fontSize: '1.2rem',
                        fontWeight: 'bold',
                        color: player.symbol === 1 ? '#ff4757' : '#2ed573'
                    }}>
                        {player.symbol === 1 ? 'X' : 'O'}
                    </span>
                    {isEditing ? (
                        <form onSubmit={handleNameSubmit} style={{ marginBottom: '15px' }}>
                            <input
                                type="text"
                                value={tempName}
                                onChange={(e) => setTempName(e.target.value)}
                                style={{
                                    backgroundColor: 'transparent',
                                    color: '#00ff00',
                                    border: '1px solid #00ff00',
                                    padding: '5px',
                                    borderRadius: '4px',
                                    fontFamily: 'monospace',
                                    width: '100%',
                                    marginBottom: '5px'
                                }}
                                autoFocus
                            />
                            <div style={{ display: 'flex', gap: '5px' }}>
                                <button
                                    type="submit"
                                    style={{
                                        backgroundColor: 'transparent',
                                        color: '#00ff00',
                                        border: '1px solid #00ff00',
                                        padding: '5px 10px',
                                        borderRadius: '4px',
                                        cursor: 'pointer',
                                        flex: 1
                                    }}
                                >
                                    Save
                                </button>
                                <button
                                    type="button"
                                    onClick={() => {
                                        setTempName(player.name);
                                        setIsEditing(false);
                                    }}
                                    style={{
                                        backgroundColor: 'transparent',
                                        color: '#00ff00',
                                        border: '1px solid #00ff00',
                                        padding: '5px 10px',
                                        borderRadius: '4px',
                                        cursor: 'pointer',
                                        flex: 1
                                    }}
                                >
                                    Cancel
                                </button>
                            </div>
                        </form>
                    ) : (
                        <span
                            onClick={() => setIsEditing(true)}
                            style={{
                                fontWeight: 'bold',
                                color: '#2f3542',
                                cursor: 'pointer',
                                marginBottom: '15px',
                                fontSize: '1.2rem',
                                textAlign: 'center',
                                textDecoration: 'underline',
                                textDecorationStyle: 'dotted'
                            }}
                        >
                            {player.name}
                        </span>
                    )}
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                    <StatItem label="Win Rate" value={`${(winRate * 100).toFixed(1)}%`} />
                    <StatItem label="Wins" value={wins} />
                    <StatItem label="Losses" value={losses} />
                    <StatItem label="Draws" value={draws} />
                    <StatItem label="Total Games" value={totalGames} />
                    <StatItem label="Best Streak" value={winStreak} />
                    <StatItem label="Current Streak" value={currentStreak} />
                    <StatItem label="Avg. Moves/Game" value={averageMovesPerGame.toFixed(1)} />
                </div>

                <button
                    onClick={() => setShowDetailedStats(true)}
                    className="button-hover"
                    style={{
                        width: '100%',
                        padding: '8px',
                        marginTop: '16px',
                        backgroundColor: player.symbol === 1 ? '#ff4757' : '#2ed573',
                        color: '#fff',
                        border: 'none',
                        borderRadius: '6px',
                        cursor: 'pointer',
                        fontSize: '0.9rem',
                        transition: 'all 0.3s ease'
                    }}
                >
                    View Detailed Stats
                </button>

                {/* Achievement badges */}
                <div style={{
                    marginTop: '12px',
                    padding: '8px',
                    backgroundColor: '#f8f9fa',
                    borderRadius: '8px',
                    fontSize: '0.8rem',
                    color: '#666',
                    textAlign: 'center'
                }}>
                    {currentStreak >= 3 && (
                        <div style={{ color: '#ff4757' }}>🔥 On Fire! ({currentStreak} wins)</div>
                    )}
                    {winRate >= 0.7 && (
                        <div style={{ color: '#2ed573' }}>👑 Dominating! ({(winRate * 100).toFixed(1)}% wins)</div>
                    )}
                </div>
            </div>

            {showDetailedStats && (
                <DetailedStats
                    player={player}
                    onClose={() => setShowDetailedStats(false)}
                />
            )}
        </>
    );
};

export default PlayerStats; 