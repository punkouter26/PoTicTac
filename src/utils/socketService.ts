import { io, Socket } from 'socket.io-client';

// Ensure the URL points to your Node.js server (running on port 3001)
// Removed process.env access as it's not available in the browser
const SERVER_URL = 'http://localhost:3001';

class SocketService {
  private socket: Socket | null = null;

  connect(): Socket {
    if (!this.socket) {
      console.log(`Connecting socket to ${SERVER_URL}`);
      this.socket = io(SERVER_URL, {
        transports: ['websocket'], // Use WebSocket transport
      });

      this.socket.on('connect', () => {
        console.log('Socket connected:', this.socket?.id);
      });

      this.socket.on('disconnect', (reason: string) => {
        console.log('Socket disconnected:', reason);
        // Optionally handle reconnection logic here
      });

      this.socket.on('connect_error', (error: Error) => {
        console.error('Socket connection error:', error);
      });
    }
    return this.socket;
  }

  disconnect(): void {
    if (this.socket) {
      console.log('Disconnecting socket...');
      this.socket.disconnect();
      this.socket = null;
    }
  }

  getSocket(): Socket | null {
    return this.socket;
  }

  // Example: Emit an event
  emit(eventName: string, data: any): void {
    this.socket?.emit(eventName, data);
  }

  // Example: Listen for an event
  on(eventName: string, callback: (...args: any[]) => void): void {
    this.socket?.on(eventName, callback);
  }

  // Example: Remove an event listener
  off(eventName: string, callback?: (...args: any[]) => void): void {
    this.socket?.off(eventName, callback);
  }
}

// Export a singleton instance
const socketService = new SocketService();
export default socketService;
