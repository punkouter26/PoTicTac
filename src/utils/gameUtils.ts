interface Position {
  x: number;
  y: number;
}

interface WinningLine {
  start: Position;
  end: Position;
}

export const getWinningLine = (winningCells: number[]): WinningLine => {
  // Calculate positions based on cell indices (0-8)
  const positions = winningCells.map(index => ({
    x: (index % 3) * 100 + 50, // 100px per cell, 50px offset for center
    y: Math.floor(index / 3) * 100 + 50
  }));

  return {
    start: positions[0],
    end: positions[positions.length - 1]
  };
}; 