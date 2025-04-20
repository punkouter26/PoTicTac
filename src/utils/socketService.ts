import { io, Socket } from 'socket.io-client';

// Connect to the server running on port 8080
const SERVER_URL = 'http://localhost:8080';

class SocketService {
  private socket: Socket | null = null;

  connect(): Socket {
    if (!this.socket) {
      console.log(`Connecting socket to ${SERVER_URL}`);
      this.socket = io(SERVER_URL, {
        transports: ['websocket', 'polling'], // Allow both WebSocket and polling
        reconnection: true, // Enable reconnection
        reconnectionAttempts: 5, // Try to reconnect 5 times
        reconnectionDelay: 1000, // Wait 1 second between attempts
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

  emit(eventName: string, data: any): void {
    if (!this.socket?.connected) {
      console.warn('Socket not connected, attempting to connect...');
      this.connect();
    }
    this.socket?.emit(eventName, data);
  }

  on(eventName: string, callback: (...args: any[]) => void): void {
    if (!this.socket?.connected) {
      console.warn('Socket not connected, attempting to connect...');
      this.connect();
    }
    this.socket?.on(eventName, callback);
  }

  off(eventName: string, callback?: (...args: any[]) => void): void {
    this.socket?.off(eventName, callback);
  }
}

// Export a singleton instance
export default new SocketService();
