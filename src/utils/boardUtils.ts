
export const BOARD_SIZE = 6;
export const WINNING_LENGTH = 4;

export function createEmptyBoard(): number[][] {
    return Array(BOARD_SIZE).fill(null)
        .map(() => Array(BOARD_SIZE).fill(0));
}

export function checkWinCondition(board: number[][], row: number, col: number, player: number): boolean {
    const directions = [
        [0, 1],   // horizontal
        [1, 0],   // vertical
        [1, 1],   // diagonal
        [1, -1]   // anti-diagonal
    ];

    return directions.some(([dx, dy]) => {
        let count = 1;
        let r, c;

        // Check forward direction
        r = row + dx;
        c = col + dy;
        while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r][c] === player) {
            count++;
            r += dx;
            c += dy;
        }

        // Check backward direction
        r = row - dx;
        c = col - dy;
        while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r][c] === player) {
            count++;
            r -= dx;
            c -= dy;
        }

        return count >= WINNING_LENGTH;
    });
}