// Create new file for sound effects
export const playSound = (type: 'move' | 'win' | 'draw') => {
    const sounds = {
        move: new Audio('/sounds/move.mp3'),
        win: new Audio('/sounds/win.mp3'),
        draw: new Audio('/sounds/draw.mp3')
    };
    sounds[type].play();
}; 