import { PlayerType, GameBoardState, LastMoveInfo, BOARD_SIZE } from '../types';
import '../styles/components.css';

interface GameBoardProps {
  gameState: GameBoardState;
  onCellClick: (row: number, col: number) => void;
  lastMove: LastMoveInfo | null;
}

/**
 * GameBoard component - renders the 6x6 game grid
 * Uses CSS Grid for responsive layout
 */
export function GameBoard({ gameState, onCellClick, lastMove }: GameBoardProps) {
  const getCellClasses = (row: number, col: number): string => {
    const value = gameState.board[row][col];
    const classes: string[] = ['cell'];

    if (value !== PlayerType.None) {
      classes.push('occupied');
      classes.push(value === PlayerType.X ? 'x-move' : 'o-move');
    }

    if (lastMove?.row === row && lastMove?.col === col) {
      classes.push('last-move');
    }

    if (gameState.winningCells?.some(([r, c]) => r === row && c === col)) {
      classes.push('winning');
    }

    return classes.join(' ');
  };

  const getCellSymbol = (value: PlayerType): string => {
    if (value === PlayerType.X) return 'X';
    if (value === PlayerType.O) return 'O';
    return '';
  };

  const handleCellClick = (row: number, col: number) => {
    if (gameState.board[row][col] === PlayerType.None) {
      onCellClick(row, col);
    }
  };

  // Determine turn class for ghost moves
  const turnClass = gameState.currentPlayer === PlayerType.X ? 'turn-x' : 'turn-o';

  return (
    <div className="game-board-container">
      <div className={`game-board retro-glow ${turnClass}`}>
        {Array.from({ length: BOARD_SIZE }, (_, row) =>
          Array.from({ length: BOARD_SIZE }, (_, col) => (
            <div
              key={`${row}-${col}`}
              className={getCellClasses(row, col)}
              onClick={() => handleCellClick(row, col)}
              data-tooltip={`Position (${row + 1}, ${col + 1})`}
              role="button"
              tabIndex={0}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  handleCellClick(row, col);
                }
              }}
              aria-label={`Cell ${row + 1}, ${col + 1}: ${getCellSymbol(gameState.board[row][col]) || 'empty'}`}
            >
              {getCellSymbol(gameState.board[row][col])}
            </div>
          ))
        )}
      </div>
    </div>
  );
}
