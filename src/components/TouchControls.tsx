import React, { useEffect, useState } from 'react';

interface TouchControlsProps {
  targetSelector: string; // CSS selector for elements that should receive touch feedback
}

const TouchControls: React.FC<TouchControlsProps> = ({ targetSelector }) => {
  const [touchFeedbacks, setTouchFeedbacks] = useState<Array<{ id: number; x: number; y: number }>>([]);
  let nextId = 0;

  useEffect(() => {
    const elements = document.querySelectorAll(targetSelector);

    // Explicitly type the event as TouchEvent
    const handleTouchStart = (event: TouchEvent) => { 
      // Access touches directly from TouchEvent
      const touch = event.touches[0]; 
      const element = event.currentTarget as HTMLElement;
      const rect = element.getBoundingClientRect();
      
      // Calculate position relative to the touched element
      const x = touch.clientX - rect.left;
      const y = touch.clientY - rect.top;
      
      // Add touch feedback effect
      const id = nextId++;
      setTouchFeedbacks(prev => [...prev, { id, x, y }]);
      
      // Remove after animation completes
      setTimeout(() => {
        setTouchFeedbacks(prev => prev.filter(feedback => feedback.id !== id));
      }, 500);
    };
    
    // Add touch event listeners to all matching elements
    elements.forEach(element => {
      // Add 'as EventListener' cast back
      element.addEventListener('touchstart', handleTouchStart as EventListener); 
    });
    
    // Clean up event listeners
    return () => {
      elements.forEach(element => {
        // Add 'as EventListener' cast back
        element.removeEventListener('touchstart', handleTouchStart as EventListener); 
      });
    };
  }, [targetSelector]);
  
  return (
    <>
      {touchFeedbacks.map(feedback => (
        <div
          key={feedback.id}
          className="touch-feedback"
          style={{
            left: `${feedback.x}px`,
            top: `${feedback.y}px`
          }}
        />
      ))}
    </>
  );
};

export default TouchControls;
