import React from 'react';
import { Difficulty } from '../utils/aiLogic';
import './DifficultySelector.css';

interface DifficultySelectorProps {
    currentDifficulty: Difficulty;
    onDifficultyChange: (difficulty: Difficulty) => void;
    disabled?: boolean;
}

export const DifficultySelector: React.FC<DifficultySelectorProps> = ({
    currentDifficulty,
    onDifficultyChange,
    disabled = false
}) => {
    const difficulties: Difficulty[] = ['easy', 'medium', 'hard'];

    return (
        <div className="difficulty-selector">
            <label className="difficulty-label">AI Difficulty:</label>
            <div className="difficulty-buttons">
                {difficulties.map((difficulty) => (
                    <button
                        key={difficulty}
                        className={`difficulty-button ${currentDifficulty === difficulty ? 'active' : ''}`}
                        onClick={() => onDifficultyChange(difficulty)}
                        disabled={disabled}
                    >
                        {difficulty.charAt(0).toUpperCase() + difficulty.slice(1)}
                    </button>
                ))}
            </div>
        </div>
    );
};
