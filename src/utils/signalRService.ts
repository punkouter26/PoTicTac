import * as signalR from '@microsoft/signalr';

// Connect to the ASP.NET Core SignalR hub
const SERVER_URL = 'http://localhost:5214/gamehub'; // Updated port to match the running server

class SignalRService {
  private connection: signalR.HubConnection | null = null;

  connect(): signalR.HubConnection {
    if (!this.connection) {
      console.log(`Connecting SignalR to ${SERVER_URL}`);
      
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(SERVER_URL)
        .withAutomaticReconnect()
        .build();

      this.connection.start()
        .then(() => {
          console.log('SignalR Connected!');
        })
        .catch(err => {
          console.error('SignalR Connection Error: ', err);
        });

      this.connection.onclose(error => {
        console.log('SignalR Connection Closed', error);
      });
    }
    
    return this.connection;
  }

  disconnect(): void {
    if (this.connection) {
      console.log('Disconnecting SignalR...');
      this.connection.stop();
      this.connection = null;
    }
  }

  getConnection(): signalR.HubConnection | null {
    return this.connection;
  }

  // Method mapping from Socket.IO to SignalR
  emit(eventName: string, data: any): Promise<any> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      console.warn('SignalR not connected, attempting to connect...');
      this.connect();
      return Promise.reject('Connection not established yet');
    }
    
    // SignalR uses method invocation instead of events
    return this.connection.invoke(eventName, ...Object.values(data));
  }

  // Method mapping from Socket.IO to SignalR
  on(eventName: string, callback: (...args: any[]) => void): void {
    if (!this.connection) {
      console.warn('SignalR not connected, attempting to connect...');
      this.connect();
    }
    
    this.connection?.on(eventName, callback);
  }

  // Method mapping from Socket.IO to SignalR
  off(eventName: string): void {
    this.connection?.off(eventName);
  }

  // Helper method to ensure connection is ready before use
  async ensureConnected(): Promise<void> {
    if (!this.connection) {
      this.connect();
    }

    if (this.connection!.state !== signalR.HubConnectionState.Connected) {
      try {
        await this.connection!.start();
      } catch (err) {
        console.error('Failed to connect to SignalR hub:', err);
        throw err;
      }
    }
  }
}

// Export a singleton instance
export default new SignalRService();