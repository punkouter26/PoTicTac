import React from 'react';
import '../styles/animations.css';

interface GameCellProps {
    value: number;
    isWinningCell: boolean;
    onClick: () => void;
    player1Color?: string;
    player2Color?: string;
}

const GameCell: React.FC<GameCellProps> = ({
    value,
    isWinningCell,
    onClick,
    player1Color = '#ff4757',
    player2Color = '#2ed573'
}) => {
    const cellClasses = [
        'cell',
        value === 0 ? 'cell-empty' : 'cell-placed',
        isWinningCell ? 'cell-winner' : ''
    ].filter(Boolean).join(' ');

    const getCellStyle = () => {
        if (value === 0) return {};
        const backgroundColor = value === 1 ? player1Color : player2Color;
        return {
            backgroundColor,
            color: '#fff',
            borderRadius: '50%',
            width: '100%',
            height: '100%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: '2rem',
            fontWeight: 'bold',
            boxShadow: isWinningCell ? `0 0 20px ${backgroundColor}` : 'none'
        };
    };

    return (
        <div 
            className={cellClasses}
            onClick={value === 0 ? onClick : undefined}
            style={{
                width: '80px',
                height: '80px',
                border: '2px solid #dfe4ea',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                cursor: value === 0 ? 'pointer' : 'default',
                position: 'relative',
                margin: '4px',
                borderRadius: '8px',
                transition: 'all 0.3s ease'
            }}
        >
            <div style={getCellStyle()}>
                {value !== 0 && (value === 1 ? 'X' : 'O')}
            </div>
        </div>
    );
};

export default GameCell; 