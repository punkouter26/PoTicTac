# PoTicTac - 4-in-a-Row Tic Tac Toe

A retro-styled 6x6 Tic Tac Toe game where players need to get 4 in a row to win. Built with Blazor WebAssembly and ASP.NET Core, featuring a retro arcade aesthetic and AI opponent.

## Features

- 6x6 game board
- Get 4 in a row to win (horizontal, vertical, or diagonal)
- Retro arcade-style UI with glowing effects
- AI opponent with three difficulty levels
- Responsive design for all devices
- Cool animations and visual feedback
- Real-time multiplayer support via SignalR

## Architecture

- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: ASP.NET Core Web API (.NET 9)
- **Database**: Azure Table Storage
- **Real-time**: SignalR
- **Hosting**: Azure App Service

### Architecture Diagrams
The solution includes Mermaid diagrams documenting the system design:

1. **Component Diagram**: Shows system components and their relationships
2. **Domain Model**: Documents the core domain entities
3. **User Workflow**: Illustrates key user flows
4. **Feature Timeline**: Shows implementation progress

To generate SVG versions of all diagrams:
1. Run the generation script: `.\generate_diagrams.ps1`
2. Alternatively, manually convert specific diagrams:
   ```bash
   npm install -g @mermaid-js/mermaid-cli
   mmdc -i Component.mermaid -o Diagrams/Component.svg
   ```

## Development

### Prerequisites
- .NET 9 SDK
- Visual Studio Code or Visual Studio 2022

### Setup
1. Clone the repository
```bash
git clone https://github.com/punkouter25/PoTicTac.git
cd PoTicTac
```

2. Restore dependencies
```bash
dotnet restore
```

3. Start development server (F5 in VS Code)
```bash
dotnet run --project server/server/PoTicTacServer
```

4. Build for production
```bash
dotnet build --configuration Release
```

### Local Development with Azurite
The project uses Azurite for local Azure Table Storage emulation during development.

## Project Structure

```
PoTicTac/
├── PoTicTac.sln                    # Solution file
├── client/PoTicTac.Client/         # Blazor WebAssembly project
├── server/server/PoTicTacServer/   # ASP.NET Core Web API project
├── tests/PoTicTac.Tests/           # XUnit test project
├── main.bicep                      # Azure deployment template
└── .vscode/                        # VS Code configuration
```

## Technologies Used

- Blazor WebAssembly
- ASP.NET Core Web API
- SignalR for real-time communication
- Azure Table Storage
- Azure App Service
- CSS3 with retro styling

## License

MIT License - feel free to use and modify as you like!
