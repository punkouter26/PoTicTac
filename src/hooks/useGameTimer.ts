
import { useState, useEffect, useCallback } from 'react';
import { TURN_TIME_LIMIT } from '../utils/gameLogic';

export const useGameTimer = (isPlaying: boolean, onTimeUp: () => void) => {
    const [timeRemaining, setTimeRemaining] = useState(TURN_TIME_LIMIT);

    const resetTimer = useCallback(() => {
        setTimeRemaining(TURN_TIME_LIMIT);
    }, []);

    useEffect(() => {
        if (!isPlaying) return;

        const timer = setInterval(() => {
            setTimeRemaining(prev => {
                if (prev <= 0) {
                    clearInterval(timer);
                    onTimeUp();
                    return 0;
                }
                return prev - 100;
            });
        }, 100);

        return () => clearInterval(timer);
    }, [isPlaying, onTimeUp]);

    return { timeRemaining, resetTimer };
};