import React from 'react';
import '../styles/animations.css';
import { Player } from '../types/GameTypes';

interface PlayerTurnIndicatorProps {
    currentPlayer: 1 | 2;
    players: [Player, Player];
}

const PlayerTurnIndicator: React.FC<PlayerTurnIndicatorProps> = ({
    currentPlayer,
    players
}) => {
    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            gap: '40px',
            margin: '20px 0'
        }}>
            {players.map((player, index) => {
                const isCurrentTurn = currentPlayer === player.symbol;
                const playerColor = player.symbol === 1 ? '#ff4757' : '#2ed573';

                return (
                    <div
                        key={player.id}
                        className={isCurrentTurn ? 'current-turn' : ''}
                        style={{
                            padding: '12px 24px',
                            backgroundColor: isCurrentTurn ? playerColor : '#f1f2f6',
                            color: isCurrentTurn ? '#fff' : '#2f3542',
                            borderRadius: '8px',
                            boxShadow: isCurrentTurn 
                                ? `0 8px 16px ${playerColor}40`
                                : '0 4px 8px rgba(0, 0, 0, 0.1)',
                            display: 'flex',
                            alignItems: 'center',
                            gap: '8px',
                            transition: 'all 0.3s ease'
                        }}
                    >
                        <span style={{
                            fontSize: '1.5rem',
                            fontWeight: 'bold'
                        }}>
                            {player.symbol === 1 ? 'X' : 'O'}
                        </span>
                        <div>
                            <div style={{ fontWeight: 'bold' }}>{player.name}</div>
                            <div style={{ fontSize: '0.8rem', opacity: 0.8 }}>
                                {isCurrentTurn ? 'Your turn' : 'Waiting...'}
                            </div>
                        </div>
                    </div>
                );
            })}
        </div>
    );
};

export default PlayerTurnIndicator; 