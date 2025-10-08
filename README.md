# PoTicTac - Retro 4-in-a-Row Tic Tac Toe

A modern, retro-styled 6x6 Tic Tac Toe game where players need 4-in-a-row to win. Built with Blazor WebAssembly and ASP.NET Core, featuring a nostalgic arcade aesthetic, intelligent AI opponents, and cloud-native architecture.

## 🎮 Project Summary

PoTicTac transforms the classic Tic Tac Toe experience into a strategic, visually compelling gaming platform. The 6x6 board with 4-in-a-row victory conditions provides significantly more strategic depth than traditional 3x3 gameplay. The application features:

- **Enhanced Strategic Gameplay**: 6x6 grid requires advanced planning and pattern recognition
- **Three-Tier AI System**: Easy (random with blocking), Medium (threat detection), Hard (minimax algorithm)
- **Retro Arcade Aesthetic**: Neon green glow effects, classic "Press Start 2P" font, smooth animations
- **Real-Time Multiplayer**: SignalR-powered live gameplay with automatic synchronization
- **Comprehensive Analytics**: Player statistics, leaderboards, win rates, and performance tracking
- **Cloud-Native Design**: Azure Table Storage, Application Insights, and scalable infrastructure
- **Full-Stack .NET 9**: Modern C# across client and server with Blazor WebAssembly

## 🏗️ Architecture

**Frontend**
- **Blazor WebAssembly (.NET 9)**: Client-side C# rendering in the browser
- **Component-Based UI**: Reusable Razor components (GameBoard, DifficultySelector)
- **Radzen.Blazor**: Professional UI component library for data grids and visualizations
- **SignalR Client**: Real-time bidirectional communication

**Backend**
- **ASP.NET Core Web API (.NET 9)**: RESTful services and SignalR hub
- **Azure Table Storage**: NoSQL database for player statistics
- **Serilog + Application Insights**: Structured logging and cloud telemetry
- **Health Checks**: Comprehensive system monitoring at `/api/health`

**Infrastructure**
- **Azure App Service**: Production hosting environment
- **Bicep IaC**: Infrastructure-as-code for repeatable deployments
- **Azurite**: Local Azure Storage emulation for development
- **GitHub Actions**: CI/CD pipeline (planned)

## 📋 Features

### Core Gameplay
- 6x6 interactive game board with smooth animations
- 4-in-a-row win detection (horizontal, vertical, diagonal)
- Visual feedback for moves, wins, and game status
- Move history tracking and undo/redo (planned)

### AI Opponent
- **Easy**: Random moves with 30% strategic blocking
- **Medium**: Threat detection and offensive pattern recognition  
- **Hard**: Minimax algorithm with alpha-beta pruning for optimal play

### Player Statistics
- Win/loss/draw tracking per player
- Win rate calculations and streak tracking
- Average moves per game analytics
- Top 10 leaderboard by win rate
- Performance history over time

### System Monitoring
- Real-time health checks for all services
- API, Storage, SignalR connection status
- Visual diagnostics dashboard at `/diag`
- Automatic service recovery and reconnection

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio Code](https://code.visualstudio.com/) or Visual Studio 2022
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (for local development)

### Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/punkouter26/PoTicTac.git
   cd PoTicTac
   ```

2. **Start Azurite (local Azure Storage emulator)**
   ```bash
   # Option 1: Using npm (if installed)
   npm install -g azurite
   azurite --silent --location ./azurite --debug ./azurite/debug.log

   # Option 2: Using Docker
   docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

   # Option 3: Visual Studio Code Extension
   # Install "Azurite" extension and start from VS Code
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   
   **Option A: Using VS Code Tasks (Recommended)**
   - Press `F5` or use "Run > Start Debugging"
   - The task automatically builds and starts the server
   - The Blazor client is served from the server project

   **Option B: Command Line**
   ```bash
   # Navigate to server project
   cd PoTicTacServer
   
   # Run the API server (includes Blazor client)
   dotnet run
   
   # Server runs on: http://localhost:5000 (HTTP) and https://localhost:5001 (HTTPS)
   ```

5. **Open in browser**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - You should see the PoTicTac main menu

### Configuration

**Local Development** (`appsettings.Development.json`):
```json
{
  "ConnectionStrings": {
    "AZURE_STORAGE_CONNECTION_STRING": "UseDevelopmentStorage=true"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "" // Leave empty for local development
  }
}
```

**Azure Production** (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "AZURE_STORAGE_CONNECTION_STRING": "<from-deployment-script>"
  },
  "ApplicationInsights": {
    "ConnectionString": "<from-deployment-script>"
  }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test PoTicTac.UnitTests/PoTicTac.UnitTests.csproj

# Run integration tests only
dotnet test PoTicTac.IntegrationTests/PoTicTac.IntegrationTests.csproj

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### Building for Production

```bash
# Build in Release mode
dotnet build --configuration Release

# Publish for deployment
dotnet publish --configuration Release --output ./publish

# Format code
dotnet format
```

## ☁️ Deploying to Azure

### Prerequisites for Azure Deployment
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)
- Active Azure subscription
- Contributor role on the subscription

### Deploy Infrastructure and Application

1. **Login to Azure**
   ```powershell
   az login
   ```

2. **Run deployment script**
   ```powershell
   .\scripts\deploy-azure.ps1
   ```

   This script will:
   - Create resource group "PoTicTac"
   - Deploy Log Analytics workspace (PerGB2018, 30-day retention)
   - Deploy Application Insights (workspace-based)
   - Deploy Storage Account (Standard_LRS) with "PlayerStats" table
   - Output connection strings to appsettings.json
   - Create .env file with connection strings

3. **Verify deployment**
   ```powershell
   # Run integration tests against Azure
   dotnet test PoTicTac.IntegrationTests/PoTicTac.IntegrationTests.csproj

   # Check Azure resources
   az resource list --resource-group PoTicTac --output table
   ```

### Cleanup Azure Resources

```powershell
# Remove all Azure resources (WARNING: Deletes everything)
.\scripts\cleanup-azure.ps1
```

### Estimated Azure Costs
- **Storage Account**: ~$0.05-0.20/month (minimal usage)
- **Log Analytics**: ~$2-5/month (pay-as-you-go, 30-day retention)
- **Application Insights**: Free tier (up to 5GB/month)
- **Total**: ~$5-10/month for typical usage

For detailed deployment documentation, see [/infra/README.md](./infra/README.md).

## 📁 Project Structure

```
PoTicTac/
├── PoTicTac.sln                       # Solution file (.NET 9)
├── prd.md                             # Product Requirements Document
├── README.md                          # This file
├── azure.yaml                         # Azure Developer CLI configuration
│
├── PoTicTac.Client/                   # Blazor WebAssembly project
│   ├── Pages/
│   │   ├── Home.razor                 # Main game page (menu + gameplay)
│   │   ├── Stats.razor                # Player statistics and leaderboard
│   │   └── Diag.razor                 # System diagnostics and health checks
│   ├── Components/
│   │   ├── GameBoard.razor            # 6x6 interactive game grid
│   │   └── DifficultySelector.razor   # AI difficulty selection component
│   ├── Services/
│   │   ├── GameLogicService.cs        # Core game mechanics and win detection
│   │   ├── AILogicService.cs          # AI opponent strategies
│   │   ├── SignalRService.cs          # Real-time multiplayer communication
│   │   └── StatisticsService.cs       # Player stats API client
│   ├── Models/
│   │   └── GameTypes.cs               # Domain models (GameState, Player, Move)
│   └── wwwroot/                       # Static assets (CSS, images)
│
├── PoTicTacServer/                    # ASP.NET Core Web API project
│   ├── Program.cs                     # Application entry point and configuration
│   ├── Controllers/
│   │   ├── HealthController.cs        # /api/health endpoint
│   │   ├── PlayersController.cs       # Player data management
│   │   └── StatisticsController.cs    # Statistics API endpoints
│   ├── Hubs/
│   │   └── GameHub.cs                 # SignalR hub for real-time multiplayer
│   ├── Services/
│   │   └── StorageService.cs          # Azure Table Storage abstraction
│   ├── Models/
│   │   └── PlayerStats.cs             # Server-side statistics models
│   ├── HealthChecks/
│   │   └── StorageHealthCheck.cs      # Custom health check for storage
│   └── appsettings.json               # Application configuration
│
├── PoTicTac.UnitTests/                # Unit tests (xUnit)
│   └── GameLogicServiceTests.cs       # Game logic unit tests
│
├── PoTicTac.IntegrationTests/         # Integration tests (xUnit)
│   ├── StatisticsControllerTests.cs   # API integration tests
│   └── AzureResourceTests.cs          # Azure storage connectivity tests
│
├── infra/                             # Infrastructure as Code (Bicep)
│   ├── main.bicep                     # Main deployment template
│   ├── resources.bicep                # Azure resource definitions
│   ├── main.bicepparam                # Deployment parameters
│   └── README.md                      # Infrastructure documentation
│
├── scripts/                           # Automation scripts
│   ├── deploy-azure.ps1               # Azure deployment automation
│   └── cleanup-azure.ps1              # Resource cleanup script
│
├── Diagrams/                          # Architecture diagrams (Mermaid)
│   ├── *.mmd                          # Mermaid diagram source files
│   └── *.svg                          # Generated SVG diagrams
│
└── .vscode/                           # VS Code configuration
    ├── tasks.json                     # Build and run tasks
    └── launch.json                    # Debug configurations
```

## 📊 Architecture Diagrams

The `/Diagrams` folder contains comprehensive Mermaid diagrams documenting the system:

1. **ProjectDependency.mmd**: .NET project dependencies and relationships
2. **ClassDiagram.mmd**: Domain entity models and their relationships
3. **SequenceDiagram.mmd**: API call flow for game moves
4. **UseCaseFlowchart.mmd**: User journey from start to game completion
5. **ComponentHierarchy.mmd**: Blazor component tree structure

To view diagrams, use the Mermaid Preview extension in VS Code or convert to SVG.

## 🛠️ Technologies Used

**Frontend**
- Blazor WebAssembly (.NET 9)
- Radzen.Blazor 8.0.3
- SignalR Client
- CSS3 with custom retro styling

**Backend**
- ASP.NET Core Web API (.NET 9)
- SignalR for real-time communication
- Serilog for structured logging
- Azure.Data.Tables SDK

**Data & Cloud**
- Azure Table Storage (NoSQL)
- Azure Application Insights
- Azure Log Analytics
- Azurite (local emulation)

**Testing**
- xUnit 2.9.3
- FluentAssertions 8.7.1
- Microsoft.AspNetCore.TestHost

**DevOps**
- Bicep for Infrastructure as Code
- Azure CLI for deployments
- PowerShell automation scripts

## 🧪 Testing Strategy

### Unit Tests (`PoTicTac.UnitTests`)
- Game logic validation (win detection, move validation)
- AI strategy correctness
- State management verification

### Integration Tests (`PoTicTac.IntegrationTests`)
- API endpoint functionality
- Azure Table Storage connectivity
- Health check validation
- End-to-end data flow

### E2E Tests (Planned)
- Playwright tests for UI workflows
- Multiplayer session testing
- Cross-browser compatibility

## 📝 Development Guidelines

### Coding Standards
- **SOLID Principles**: Enforced throughout the codebase
- **Design Patterns**: GoF patterns where appropriate
- **File Size Limit**: Maximum 500 lines per file
- **Naming Convention**: `Po.AppName.*` for all projects
- **Test-Driven Development**: Write tests before implementation

### Code Quality
- Run `dotnet format` before committing
- Ensure all tests pass: `dotnet test`
- Check for build warnings: `dotnet build`
- Use meaningful commit messages

### Architecture Patterns
- **Vertical Slice Architecture**: Features organized by use case
- **Clean Architecture**: Separation of concerns and dependencies
- **Service Layer Pattern**: Business logic in dedicated services
- **Repository Pattern**: Data access abstraction (StorageService)

## 🐛 Troubleshooting

### Common Issues

**1. "Connection refused" errors**
- Ensure Azurite is running: `azurite --version`
- Check connection string in `appsettings.Development.json`
- Verify ports 10000-10002 are not blocked

**2. "Table not found" errors**
- Azurite tables are created automatically on first write
- Check Azurite logs for errors
- Try restarting Azurite with `--location ./azurite`

**3. SignalR connection failures**
- Verify server is running on correct port (5000/5001)
- Check browser console for WebSocket errors
- Ensure HTTPS is configured properly

**4. Build errors after package updates**
- Clean solution: `dotnet clean`
- Restore packages: `dotnet restore`
- Rebuild: `dotnet build`

### Diagnostic Tools

- **Health Check Endpoint**: `https://localhost:5001/api/health`
- **Diagnostics Page**: `https://localhost:5001/diag`
- **Swagger UI**: `https://localhost:5001/swagger` (development only)
- **Azurite Explorer**: Use Azure Storage Explorer with local connection

For more troubleshooting, see [/infra/README.md](./infra/README.md).

## 🎯 Roadmap

### Current Phase (v1.0)
- ✅ Core 6x6 gameplay with 4-in-a-row
- ✅ Three-tier AI system
- ✅ Player statistics and leaderboards
- ✅ Azure cloud deployment
- ⏳ Multiplayer lobby system
- ⏳ End-to-end Playwright tests

### Future Enhancements (v2.0)
- Tournament mode with brackets
- Custom board sizes (4x4, 8x8)
- Multiple visual themes
- Mobile-responsive improvements
- Advanced analytics dashboard

### Long-term Vision (v3.0)
- Native mobile apps (iOS/Android)
- Social features and friend systems
- ELO-based competitive ranking
- AI training with user data

## 📄 License

MIT License - feel free to use, modify, and distribute as you like!

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow the coding standards in this README
4. Write tests for new functionality
5. Submit a pull request

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/punkouter26/PoTicTac/issues)
- **Documentation**: See `/prd.md` for detailed product requirements
- **Architecture**: See `/Diagrams` folder for system design

---

**Built with ❤️ using .NET 9, Blazor WebAssembly, and Azure**
