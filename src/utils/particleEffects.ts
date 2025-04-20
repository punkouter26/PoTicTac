interface ParticleOptions {
    x: number;
    y: number;
    color?: string;
    count?: number;
    size?: number;
    speed?: number;
}

export const createParticles = (options: ParticleOptions) => {
    const {
        x,
        y,
        color = '#00ff00',
        count = 10,
        size = 8,
        speed = 1
    } = options;

    const particles: HTMLElement[] = [];

    for (let i = 0; i < count; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        particle.style.width = `${size}px`;
        particle.style.height = `${size}px`;
        particle.style.background = color;
        particle.style.left = `${x}px`;
        particle.style.top = `${y}px`;
        particle.style.animationDuration = `${speed}s`;
        
        // Random direction
        const angle = Math.random() * Math.PI * 2;
        const velocity = 50 + Math.random() * 50;
        const vx = Math.cos(angle) * velocity;
        const vy = Math.sin(angle) * velocity;
        
        particle.style.setProperty('--vx', `${vx}px`);
        particle.style.setProperty('--vy', `${vy}px`);
        
        document.body.appendChild(particle);
        particles.push(particle);
        
        // Remove particle after animation
        particle.addEventListener('animationend', () => {
            particle.remove();
        });
    }

    return particles;
};

export const createWinEffect = (x: number, y: number) => {
    return createParticles({
        x,
        y,
        color: '#00ff00',
        count: 20,
        size: 12,
        speed: 1.5
    });
};

export const createDrawEffect = (x: number, y: number) => {
    return createParticles({
        x,
        y,
        color: '#ffff00',
        count: 15,
        size: 10,
        speed: 1.2
    });
};

export const createMoveEffect = (x: number, y: number, playerSymbol: number) => {
    return createParticles({
        x,
        y,
        color: playerSymbol === 1 ? '#00ff00' : '#ff0000',
        count: 5,
        size: 6,
        speed: 0.8
    });
}; 