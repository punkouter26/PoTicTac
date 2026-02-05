# PoTicTac - Retro 4-in-a-Row Tic Tac Toe

A modern, retro-styled 6x6 Tic Tac Toe game where players need 4-in-a-row to win. Built with **React.js** frontend and **ASP.NET Core** API backend.

## 🎮 Project Summary

PoTicTac transforms the classic Tic Tac Toe experience into a strategic, visually compelling gaming platform. The 6x6 board with 4-in-a-row victory conditions provides significantly more strategic depth than traditional 3x3 gameplay. The application features:

- **Enhanced Strategic Gameplay**: 6x6 grid requires advanced planning and pattern recognition
- **Three-Tier AI System**: Easy (random with blocking), Medium (threat detection), Hard (minimax algorithm)
- **Retro Arcade Aesthetic**: Neon glow effects, modern dark theme, smooth animations
- **Real-Time Multiplayer**: SignalR-powered live gameplay (coming soon)
- **Comprehensive Analytics**: Player statistics, leaderboards, win rates, and performance tracking
- **Cloud-Native Design**: Azure Table Storage for statistics persistence
- **Modern Stack**: React 19 frontend with .NET 10 API backend

## 🏗️ Architecture

**Frontend (React.js)**
- **React 19 with TypeScript**: Modern component-based UI
- **Vite**: Fast development and optimized production builds
- **Client-side AI**: Game logic runs entirely in browser for responsive gameplay
- **Offline Support**: App works without API connection (local storage fallback)

**Backend (.NET 10 API)**
- **ASP.NET Core Minimal APIs**: RESTful services for statistics and leaderboards
- **Azure Table Storage**: NoSQL database for player statistics
- **Serilog**: Structured logging
- **Health Checks**: Comprehensive system monitoring at `/health`
- **SignalR Hub**: Real-time communication for multiplayer (coming soon)

## 📋 Features

### Core Gameplay
- 6x6 interactive game board with smooth animations
- 4-in-a-row win detection (horizontal, vertical, diagonal)
- Visual feedback for moves, wins, and game status
- Move history tracking

### AI Opponent
- **Easy**: Random moves with 30% strategic blocking
- **Medium**: Threat detection and offensive pattern recognition  
- **Hard**: Minimax algorithm with alpha-beta pruning for optimal play

### Player Statistics
- Win/loss/draw tracking per player
- Win rate calculations and streak tracking
- Average moves per game analytics
- Top 10 leaderboard by win rate
- Performance breakdown by difficulty

### System Monitoring
- Health checks for API and storage
- Structured logging with Serilog

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22+](https://nodejs.org/)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (for local storage emulation)

### Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/punkouter26/PoTicTac.git
   cd PoTicTac
   ```

2. **Install React dependencies**
   ```bash
   cd src/Po.TicTac.Web
   npm install
   cd ../..
   ```

3. **Start Azurite** (for local storage emulation)
   ```bash
   # Using npm
   npx azurite --silent --location ./azurite-data

   # Or using Docker
   docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
   ```

4. **Start the API**
   ```bash
   dotnet run --project src/Po.TicTac.Api
   ```
   The API will be available at `https://localhost:5001`

5. **Start the React frontend** (in a new terminal)
   ```bash
   cd src/Po.TicTac.Web
   npm run dev
   ```
   The app will be available at `http://localhost:3000`

6. **Open in browser**
   - Navigate to `http://localhost:3000` for the game
   - Navigate to `https://localhost:5001/swagger` for API docs

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Health check with detailed status |
| `/api/statistics` | GET | Get all player statistics |
| `/api/statistics/leaderboard?limit=10` | GET | Get top players by win rate |
| `/api/players/{name}/stats` | GET | Get specific player stats |
| `/api/players/{name}/stats` | PUT | Save/update player stats |
| `/swagger` | GET | Swagger UI for API exploration |

### Running Tests

```bash
# .NET Unit Tests
dotnet test tests/Po.TicTac.UnitTests/

# .NET Integration Tests (requires Azurite running)
dotnet test tests/Po.TicTac.IntegrationTests/

# React Unit Tests
cd src/Po.TicTac.Web
npm test

# React Tests with Coverage
npm run test:coverage
```

## 📁 Project Structure

```
src/
├── Po.TicTac.Api/          # ASP.NET Core REST API
│   ├── Features/           # Vertical slice feature modules
│   ├── Models/             # Domain models and DTOs
│   ├── DTOs/               # Data transfer objects
│   ├── Services/           # Business logic services
│   ├── Hubs/               # SignalR hubs
│   └── HealthChecks/       # Custom health checks
├── Po.TicTac.Web/          # React.js frontend
│   ├── src/
│   │   ├── components/     # React components
│   │   ├── pages/          # Page components
│   │   ├── services/       # API clients and game logic
│   │   ├── styles/         # CSS stylesheets
│   │   └── types/          # TypeScript type definitions
│   └── package.json
tests/
├── Po.TicTac.UnitTests/         # C# unit tests
├── Po.TicTac.IntegrationTests/  # API integration tests
└── Po.TicTac.E2ETests/          # Playwright E2E tests
```

## 🔧 Configuration

### API Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Tables": "UseDevelopmentStorage=true"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://localhost:3000"]
  },
  "KeyVault": {
    "Uri": "https://your-keyvault.vault.azure.net/"
  }
}
```

### React Configuration

The React app uses Vite with a proxy configuration in `vite.config.ts` to route API calls to the backend during development.

## 🚢 Deployment

### Azure Container Apps

Deploy using Azure CLI:

```bash
# Build and push Docker images
docker build -t potictac-api:latest -f src/Po.TicTac.Api/Dockerfile .
docker build -t potictac-web:latest -f src/Po.TicTac.Web/Dockerfile .

# Deploy to Azure Container Apps
az containerapp create --name potictac-api ...
az containerapp create --name potictac-web ...
```

### Azure Static Web Apps (React Frontend)

The React frontend can also be deployed to Azure Static Web Apps for a serverless hosting option.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
