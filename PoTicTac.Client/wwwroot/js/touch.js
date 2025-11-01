// Touch Gestures Handler for PoTicTac

let touchStartX = 0;
let touchEndX = 0;
let touchStartY = 0;
let touchEndY = 0;
let dotnetHelper = null;

// Minimum swipe distance in pixels
const SWIPE_THRESHOLD = 100;
const VERTICAL_TOLERANCE = 50;

/**
 * Initialize swipe gesture detection
 * @param {any} helper - DotNet object reference for callbacks
 */
window.initSwipeGestures = function(helper) {
    dotnetHelper = helper;
    
    document.addEventListener('touchstart', handleTouchStart, { passive: true });
    document.addEventListener('touchend', handleTouchEnd, { passive: true });
    
    console.log('Swipe gestures initialized');
};

/**
 * Cleanup swipe gesture listeners
 */
window.cleanupSwipeGestures = function() {
    document.removeEventListener('touchstart', handleTouchStart);
    document.removeEventListener('touchend', handleTouchEnd);
    dotnetHelper = null;
    console.log('Swipe gestures cleaned up');
};

/**
 * Handle touch start event
 * @param {TouchEvent} event 
 */
function handleTouchStart(event) {
    touchStartX = event.changedTouches[0].screenX;
    touchStartY = event.changedTouches[0].screenY;
}

/**
 * Handle touch end event and detect swipe direction
 * @param {TouchEvent} event 
 */
function handleTouchEnd(event) {
    touchEndX = event.changedTouches[0].screenX;
    touchEndY = event.changedTouches[0].screenY;
    
    handleSwipe();
}

/**
 * Determine swipe direction and invoke appropriate callback
 */
function handleSwipe() {
    if (!dotnetHelper) return;
    
    const deltaX = touchStartX - touchEndX;
    const deltaY = touchStartY - touchEndY;
    const absDeltaX = Math.abs(deltaX);
    const absDeltaY = Math.abs(deltaY);
    
    // Check if horizontal swipe (and not too vertical)
    if (absDeltaX > SWIPE_THRESHOLD && absDeltaX > absDeltaY) {
        if (deltaX > 0) {
            // Swipe left
            dotnetHelper.invokeMethodAsync('OnSwipeLeft')
                .catch(err => console.log('Swipe left handler not implemented'));
        } else {
            // Swipe right
            dotnetHelper.invokeMethodAsync('OnSwipeRight')
                .catch(err => console.log('Swipe right handler not implemented'));
        }
    }
    
    // Check if vertical swipe
    if (absDeltaY > SWIPE_THRESHOLD && absDeltaY > absDeltaX) {
        if (deltaY > 0) {
            // Swipe up
            dotnetHelper.invokeMethodAsync('OnSwipeUp')
                .catch(err => console.log('Swipe up handler not implemented'));
        } else {
            // Swipe down
            dotnetHelper.invokeMethodAsync('OnSwipeDown')
                .catch(err => console.log('Swipe down handler not implemented'));
        }
    }
}

/**
 * Add visual touch feedback to an element
 * @param {HTMLElement} element 
 */
window.addTouchFeedback = function(element) {
    if (!element) return;
    
    element.addEventListener('touchstart', function() {
        this.classList.add('touch-active');
    }, { passive: true });
    
    element.addEventListener('touchend', function() {
        this.classList.remove('touch-active');
    }, { passive: true });
    
    element.addEventListener('touchcancel', function() {
        this.classList.remove('touch-active');
    }, { passive: true });
};

/**
 * Enable long press detection on an element
 * @param {HTMLElement} element 
 * @param {Function} callback 
 * @param {number} duration - Long press duration in ms (default 500ms)
 */
window.enableLongPress = function(element, callback, duration = 500) {
    if (!element) return;
    
    let pressTimer;
    
    element.addEventListener('touchstart', function(e) {
        pressTimer = setTimeout(() => {
            if (callback) callback(e);
        }, duration);
    }, { passive: true });
    
    element.addEventListener('touchend', function() {
        clearTimeout(pressTimer);
    }, { passive: true });
    
    element.addEventListener('touchmove', function() {
        clearTimeout(pressTimer);
    }, { passive: true });
};

// CSS for touch feedback
const touchStyles = `
.touch-active {
    transform: scale(0.95);
    filter: brightness(1.2);
    transition: transform 0.1s ease, filter 0.1s ease;
}

.touch-pulse::after {
    content: '';
    position: absolute;
    top: 50%;
    left: 50%;
    width: 100%;
    height: 100%;
    border-radius: inherit;
    transform: translate(-50%, -50%);
    background: rgba(0, 255, 0, 0.3);
    animation: touch-pulse-animation 0.3s ease-out;
    pointer-events: none;
}

@keyframes touch-pulse-animation {
    0% {
        transform: translate(-50%, -50%) scale(0);
        opacity: 1;
    }
    100% {
        transform: translate(-50%, -50%) scale(2);
        opacity: 0;
    }
}
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = touchStyles;
document.head.appendChild(styleSheet);
