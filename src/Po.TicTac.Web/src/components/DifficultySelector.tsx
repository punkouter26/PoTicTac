import { Difficulty } from '../types';
import '../styles/components.css';

interface DifficultySelectorProps {
  currentDifficulty: Difficulty;
  onDifficultyChange: (difficulty: Difficulty) => void;
}

/**
 * DifficultySelector component - allows player to select AI difficulty
 * Uses Strategy pattern reference for game mechanics
 */
export function DifficultySelector({
  currentDifficulty,
  onDifficultyChange,
}: DifficultySelectorProps) {
  const difficulties: { value: Difficulty; label: string; description: string }[] = [
    { value: Difficulty.Easy, label: '🟢 Easy', description: 'Random moves with occasional blocking' },
    { value: Difficulty.Medium, label: '🟡 Medium', description: 'Smart threat detection' },
    { value: Difficulty.Hard, label: '🔴 Hard', description: 'Minimax AI - Nearly unbeatable!' },
  ];

  return (
    <div className="difficulty-selector">
      <label className="section-label">Difficulty</label>
      <div className="difficulty-buttons">
        {difficulties.map((diff) => (
          <button
            key={diff.value}
            className={`difficulty-button ${currentDifficulty === diff.value ? 'selected' : ''}`}
            onClick={() => onDifficultyChange(diff.value)}
            title={diff.description}
            aria-pressed={currentDifficulty === diff.value}
          >
            {diff.label}
          </button>
        ))}
      </div>
    </div>
  );
}
