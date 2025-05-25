using Microsoft.AspNetCore.SignalR.Client;
using PoTicTac.Client.Models;

namespace PoTicTac.Client.Services
{
    public class SignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public SignalRService()
        {
            _hubUrl = "/gamehub"; // Relative URL since client is hosted by server
        }

        public async Task<HubConnection> ConnectAsync()
        {
            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();

                await _hubConnection.StartAsync();
            }

            return _hubConnection;
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        // Event handlers
        public void OnMoveReceived(Action<GameState, Move> handler)
        {
            _hubConnection?.On<GameState, Move>("MoveReceived", handler);
        }

        public void OnGameOver(Action<GameState> handler)
        {
            _hubConnection?.On<GameState>("GameOver", handler);
        }

        public void OnRematchRequested(Action<string> handler)
        {
            _hubConnection?.On<string>("RematchRequested", handler);
        }

        public void OnRematchAccepted(Action<GameState> handler)
        {
            _hubConnection?.On<GameState>("RematchAccepted", handler);
        }

        public void OnPlayerLeft(Action<string> handler)
        {
            _hubConnection?.On<string>("PlayerLeft", handler);
        }

        public void OnGameJoined(Action<string, bool, GameState> handler)
        {
            _hubConnection?.On<string, bool, GameState>("GameJoined", handler);
        }

        public void OnGameCreated(Action<string> handler)
        {
            _hubConnection?.On<string>("GameCreated", handler);
        }

        public void OnPlayerJoined(Action<string, string> handler)
        {
            _hubConnection?.On<string, string>("PlayerJoined", handler);
        }

        // Hub method calls
        public async Task CreateGameAsync(string playerName)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("CreateGame", playerName);
            }
        }

        public async Task JoinGameAsync(string gameId, string playerName)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("JoinGame", gameId, playerName);
            }
        }

        public async Task MakeMoveAsync(string gameId, int row, int col)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("MakeMove", new { gameId, row, col });
            }
        }

        public async Task LeaveGameAsync(string gameId)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("LeaveGame", new { gameId });
            }
        }

        public async Task RequestRematchAsync(string gameId)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("RequestRematch", new { gameId });
            }
        }

        public async Task AcceptRematchAsync(string gameId)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("AcceptRematch", new { gameId });
            }
        }

        // Remove event handlers
        public void RemoveHandler(string methodName)
        {
            _hubConnection?.Remove(methodName);
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
